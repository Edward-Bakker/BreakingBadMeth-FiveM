using CitizenFX.Core;
using CitizenFX.Core.Native;
using Client.Util;
using static CitizenFX.Core.Native.API;
using System;

namespace Client
{
    public class Main : BaseScript
    {
        dynamic ESX;
        int LastVehicle;
        int CurrentVehicle;

        bool Started;
        bool Displayed;
        bool Pause;
        int Progress;
        int Selection;
        int Quality;

        #region Good code

        public Main()
        {
            TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx =>
            {
                ESX = esx;
            })});

            EventHandlers["esx_methcar:startprod"] += new Action(StartProduction);
            EventHandlers["esx_methcar:stop"] += new Action(StopProduction);
            EventHandlers["esx_methcar:stopfreeze"] += new Action<int>(StopFreeze);
            EventHandlers["esx_methcar:blowup"] += new Action<float, float, float>(BlowUpVehicle);
            EventHandlers["esx_methcar:smoke"] += new Action<float, float, float>(VehicleSmoke);
            EventHandlers["esx_methcar:drugged"] += new Action(Drugged);

            StartProduction();
            VehicleCheck();
            SelectionInput();
        }

        private void DisplayHelpText(string str)
        {
            BeginTextCommandDisplayHelp("STRING");
            AddTextComponentString(str);
            DisplayHelpTextFromStringLabel(0, false, true, -1);
        }

        private void StartProduction()
        {
            DisplayHelpText("~g~Starting production");
            Started = true;
            FreezeEntityPosition(CurrentVehicle, true);
            Debug.WriteLine("Started meth production");
            ESX.ShowNotification("~r~Meth production has started");
            SetPedIntoVehicle(GetPlayerPed(-1), CurrentVehicle, 3);
            SetVehicleDoorOpen(CurrentVehicle, 2, true, false);
        }

        private void StopProduction()
        {
            Started = false;
            DisplayHelpText("~r~Production stopped...");
            FreezeEntityPosition(LastVehicle, false);
        }

        private void StopFreeze(int id)
        {
            FreezeEntityPosition(id, false);
        }

        private void Notify(string message)
        {
            ESX.ShowNotification(message);
        }

        private async void BlowUpVehicle(float posx, float posy, float posz)
        {
            AddExplosion(posx, posy, posz + 2, 23, 20f, true, false, 1f);
            if (!HasNamedPtfxAssetLoaded("core"))
            {
                RequestNamedPtfxAsset("core");
                while (!HasNamedPtfxAssetLoaded("core"))
                {
                    await Delay(100);
                }
            }
            UseParticleFxAsset("core");
            int fire = StartParticleFxLoopedAtCoord("ent_ray_heli_aprtmnt_l_fire", posx, posy, posz - 0.8f, 0f, 0f, 0f, 0.8f, false, false, false, false);
            await Delay(6000);
            StopParticleFxLooped(fire, false);
        }

        private async void VehicleSmoke(float posx, float posy, float posz)
        {
            if (!HasNamedPtfxAssetLoaded("core"))
            {
                RequestNamedPtfxAsset("core");
                while (!HasNamedPtfxAssetLoaded("core"))
                {
                    await Delay(100);
                }
            }
            UseParticleFxAsset("core");
            int smoke = StartParticleFxLoopedAtCoord("exp_grd_flare", posx, posy, posz + 1.7f, 0f, 0f, 0f, 2f, false, false, false, false);
            SetParticleFxLoopedAlpha(smoke, 0.8f);
            SetParticleFxLoopedColour(smoke, 0f, 0f, 0f, false);
            await Delay(22000);
            StopParticleFxLooped(smoke, false);
        }

        private async void Drugged()
        {
            SetTimecycleModifier("drug_drive_bled01");
            SetPedMotionBlur(GetPlayerPed(-1), true);
            SetPedMovementClipset(GetPlayerPed(-1), "MOVE_M@DRUNK@SLIGHTLYDRUNK", 1f);
            SetPedIsDrunk(GetPlayerPed(-1), true);

            await Delay(300000);
            ClearTimecycleModifier();
        }

        #endregion

        #region Bad code

        private async void StartCooking()
        {
            while (true)
            {

                await Delay(10);
                int playerPed = GetPlayerPed(-1);
                Vector3 pos = GetEntityCoords(playerPed, true);

                if (IsPedSittingInAnyVehicle(playerPed))
                {
                    CurrentVehicle = GetVehiclePedIsUsing(PlayerPedId());

                    int vehicle = GetVehiclePedIsIn(playerPed, false);
                    LastVehicle = GetVehiclePedIsUsing(playerPed);

                    string modelName = GetDisplayNameFromVehicleModel((uint)GetEntityModel(CurrentVehicle));

                    if (modelName == "JOURNEY" && vehicle >= 1)
                    {
                        if (GetPedInVehicleSeat(vehicle, -1) == playerPed)
                        {
                            if (!Started)
                            {
                                if (!Displayed)
                                {
                                    DisplayHelpText("Press ~INPUT_THROW_GRENADE~ to start making meth.");
                                    Displayed = true;
                                }
                            }
                            if (IsControlJustReleased(0, (int)Keys.G))
                            {
                                if (pos.Y >= 3500)
                                {
                                    if (IsVehicleSeatFree(CurrentVehicle, 3))
                                    {
                                        TriggerServerEvent("esx_methcar:start");
                                        Pause = false;
                                        Progress = 0;
                                        Selection = 0;
                                        Quality = 0;
                                    }
                                    else
                                    {
                                        DisplayHelpText("~r~The car is already occupied");
                                    }
                                }
                                else
                                {
                                    ESX.ShowNotification("~r~You are too close to the city, head further up north to begin meth production.");
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Started)
                    {
                        Started = false;
                        Displayed = false;
                        TriggerEvent("esx_methcar:stop");
                        Debug.WriteLine("Stopped making drugs");
                        FreezeEntityPosition(LastVehicle, false);
                    }
                }

                if (Started)
                {
                    if (Progress < 100)
                    {
                        await Delay(6000);
                        if (!Pause && IsPedInAnyVehicle(playerPed, false))
                        {
                            Progress++;
                            ESX.ShowNotification($"~r~Meth production: ~g~~h~ {Progress} % ");
                            await Delay(6000);
                        }

                        // EVENT 1
                        if (Progress == 23)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~The propane pipe is leaking, what do you do?");
                                ESX.ShowNotification("~o~1. Fix using tape");
                                ESX.ShowNotification("~o~2. Leave it be");
                                ESX.ShowNotification("~o~3. Replace it");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotification("~r~The tape kind of stopped the leak");
                                Pause = false;
                                Quality += 3;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("Selected 2");
                                ESX.ShowNotification("~r~The propane tank blew up, you messed up...");
                                SetVehicleEngineHealth(CurrentVehicle, 0f);
                                Started = false;
                                Displayed = false;
                                Quality = 0;
                                TriggerServerEvent("esx_methcar:blow", pos.X, pos.Y, pos.Z);
                                ApplyDamageToPed(playerPed, 10, false);
                                Debug.WriteLine("Stopped producing drugs");
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotification("~r~Good job, the pipe wasn't in good condition");
                                Pause = false;
                                Quality += 5;
                            }
                        }
                        // Event 2
                        else if (Progress == 31)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~You spilled a bottle of acetone on the ground, what do you do?");
                                ESX.ShowNotification("~o~1. Open the windows to get rid of the smell");
                                ESX.ShowNotification("~o~2. Leave it be");
                                ESX.ShowNotification("~o~3. Put on a mask with airfilter");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotificaiton("~r~You opened the windows to get rid of the smell");
                                Pause = false;
                                Quality--;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("Selected 2");
                                ESX.ShowNotification("~r~You got high from inhaling acetone too much");
                                Pause = false;
                                TriggerEvent("esx_methcar:drugged");
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotificaiton("~r~That's an easy way to fix the issue... I guess");
                                SetPedPropIndex(playerPed, 1, 26, 7, true);
                                Pause = false;
                            }
                        }
                        // Event 3
                        else if (Progress == 39)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~Meth becomes solid too fast, what do you do?");
                                ESX.ShowNotification("~o~1. Raise the pressure");
                                ESX.ShowNotification("~o~2. Raise the temperature");
                                ESX.ShowNotification("~o~3. Lower the pressure");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotification("~r~You raised the pressure and the propane started escaping, you lowered it and it's okay for now");
                                Pause = false;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("Selected 2");
                                ESX.ShowNotification("~r~Raising the temperature helped...");
                                Pause = false;
                                Quality += 5;
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotification("~r~Lowering the pressure just made it worse...");
                                Quality -= 4;
                                Pause = false;
                            }
                        }
                        // Event 4
                        else if (Progress == 42)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~You accidentally pour too much acetone, what do you do?");
                                ESX.ShowNotification("~o~1. Do nothing");
                                ESX.ShowNotification("~o~2. Try sucking it out using a syringe");
                                ESX.ShowNotification("~o~3. Add more lithium to balance it out");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotification("~r~The meth is now smelling like acetone");
                                Pause = false;
                                Quality -= 3;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("Selected 2");
                                ESX.ShowNotification("~r~It kind of worked, but it's still too much");
                                Pause = false;
                                Quality--;
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotification("~r~You successfully balanced both chemicals out and it's good again");
                                Pause = false;
                                Quality -= 3;
                            }
                        }
                        // Event 5
                        else if (Progress == 48)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~You found some water coloring, what do you do?");
                                ESX.ShowNotification("~o~1. Add it in");
                                ESX.ShowNotification("~o~2. Put it away");
                                ESX.ShowNotification("~o~3. Drink it");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotification("~r~Good idea, people like colors");
                                Pause = false;
                                Quality += 4;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("Selected 2");
                                ESX.ShowNotification("~r~Yeah it might destroy the taste of meth");
                                Pause = false;
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotification("~r~You are a bit weird and feel dizzy but it's all good");
                                Pause = false;
                            }
                        }
                        // Event 6
                        else if (Progress == 56)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~The filter is clogged, what do you do?");
                                ESX.ShowNotification("~o~1. Clean it using compressed air");
                                ESX.ShowNotification("~o~2. Replace the filter");
                                ESX.ShowNotification("~o~3. Clean it using a tooth brush");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotification("~r~Compressed air sprayed the liquid meth all over you");
                                Pause = false;
                                Quality -= 2;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("Selected 2");
                                ESX.ShowNotification("~r~Replacing it was probably the best option");
                                Pause = false;
                                Quality += 3;
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotification("~r~This worked quite well but its still kinda dirty");
                                Pause = false;
                                Quality--;
                            }
                        }
                        // Event 6
                        else if (Progress == 59)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~You spilled a bottle of acetone on the ground, what do you do?");
                                ESX.ShowNotification("~o~1. Open the windows to get rid of the smell");
                                ESX.ShowNotification("~o~2. Leave it be");
                                ESX.ShowNotification("~o~3. Put on a mask with airfilter");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotification("~r~You opened the windows to get rid of the smell");
                                Pause = false;
                                Quality--;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("Selected 2");
                                ESX.ShowNotification("~r~You got high from inhaling acetone too much");
                                Pause = false;
                                TriggerEvent("esx_methcar:drugged");
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotification("~r~That's an easy way to fix the issue... I guess");
                                Pause = false;
                                SetPedPropIndex(playerPed, 1, 26, 7, true);
                            }
                        }
                        // Event 7
                        else if (Progress == 64)
                        {
                            Pause = true;
                            if (Selection == 0)
                            {
                                ESX.ShowNotification("~o~The propane pipe is leaking, what do you do?");
                                ESX.ShowNotification("~o~1. Fix using tape");
                                ESX.ShowNotification("~o~2. Leave it be");
                                ESX.ShowNotification("~o~3. Replace it");
                                ESX.ShowNotification("~c~Press the number of the option you want to do");
                            }
                            else if (Selection == 1)
                            {
                                Debug.WriteLine("Selected 1");
                                ESX.ShowNotification("~r~The tape kind of stopped the leak");
                                Pause = false;
                                Quality -= 3;
                            }
                            else if (Selection == 2)
                            {
                                Debug.WriteLine("~r~The propane tank blew up, you messed up...");
                                SetVehicleEngineHealth(CurrentVehicle, 0f);
                                Quality = 0;
                                Started = false;
                                Displayed = false;
                                TriggerServerEvent("esx_methcar:blow", pos.X, pos.Y, pos.Z);
                                ApplyDamageToPed(playerPed, 10, false);
                                Debug.WriteLine("Stopped producing drugs");
                            }
                            else if (Selection == 3)
                            {
                                Debug.WriteLine("Selected 3");
                                ESX.ShowNotification("~r~Good job, the pipe wasn't in a good condition");
                                Pause = false;
                                Quality += 5;
                            }
                        }
                        // Event 8
                        else if (Progress == 72)
                        {
                            Pause = true;
                            switch (Selection)
                            {
                                case 1:
                                    Debug.WriteLine("Selected 1");
                                    ESX.ShowNotification("~r~Compressed air sprayed the liquid meth all over you");
                                    Pause = false;
                                    Quality -= 2;
                                    break;
                                case 2:
                                    Debug.WriteLine("Selected 2");
                                    ESX.ShowNotification("~r~Replacing it was probably the best option");
                                    Pause = false;
                                    Quality += 3;
                                    break;
                                case 3:
                                    Debug.WriteLine("Selected 3");
                                    ESX.ShowNotification("~o~This worked quite well but it's still kind of dirty");
                                    Pause = false;
                                    Quality--;
                                    break;
                                default:
                                    ESX.ShowNotification("~o~The filter is clogged, what do you do?");
                                    ESX.ShowNotification("~o~1. Clean it using compressed air");
                                    ESX.ShowNotification("~o~2. Replace the filter");
                                    ESX.ShowNotification("~o~3. Clean it using a tooth brush");
                                    ESX.ShowNotification("~c~Press the number of the option you want to do");
                                    break;
                            }
                        }
                        // Event 9
                        else if (Progress == 77)
                        {
                            Pause = true;
                            switch (Selection)
                            {
                                case 1:
                                    Debug.WriteLine("Selected 1");
                                    ESX.ShowNotification("~r~The meth now smells like acetone");
                                    Pause = false;
                                    Quality -= 3;
                                    break;
                                case 2:
                                    Debug.WriteLine("Selected 2");
                                    ESX.ShowNotification("~r~It kind of worked but it's still too much");
                                    Pause = false;
                                    Quality--;
                                    break;
                                case 3:
                                    Debug.WriteLine("Selected 3");
                                    ESX.ShowNotification("~r~You successfully balanced both chemicals out and it's good again");
                                    Pause = false;
                                    Quality += 3;
                                    break;
                                default:
                                    ESX.ShowNotification("~o~You accidentally pour too much acetone, what do you?");
                                    ESX.ShowNotification("~o~1. Do nothing");
                                    ESX.ShowNotification("~o~2. Try to suck it out using a syringe");
                                    ESX.ShowNotification("~o~3. Add more lithium to balance it out");
                                    ESX.ShowNotification("~c~Press the number of the option you want to do");
                                    break;
                            }
                        }
                        // Event 10
                        else if (Progress == 83)
                        {
                            Pause = true;
                            switch (Selection)
                            {
                                case 1:
                                    Debug.WriteLine("Selected 1");
                                    ESX.ShowNotification("~r~Good job, you need to work first, shit later");
                                    Pause = false;
                                    Quality++;
                                    break;
                                case 2:
                                    Debug.WriteLine("Selected 2");
                                    ESX.ShowNotification("~r~While you were outside the glas fell off the table and spilled all over the floor...");
                                    Pause = false;
                                    Quality -= 2;
                                    break;
                                case 3:
                                    Debug.WriteLine("Selected 3");
                                    ESX.ShowNotification("~r~The air smells like shit now, the meth smells like shit now");
                                    Pause = false;
                                    Quality--;
                                    break;
                                default:
                                    ESX.ShowNotification("~o~You need to take a shit, what do you do?");
                                    ESX.ShowNotification("~o~1. Try to hold it");
                                    ESX.ShowNotification("~o~2. Go outside and take a shit");
                                    ESX.ShowNotification("~o~3. Shit inside");
                                    ESX.ShowNotification("~c~Press the number of the option you want to do");
                                    break;
                            }
                        }
                        // Event 11
                        else if (Progress == 89)
                        {
                            Pause = true;
                            switch (Selection)
                            {
                                case 1:
                                    Debug.WriteLine("Selected 1");
                                    ESX.ShowNotification("~r~Now you get a few more baggies out of it");
                                    Pause = false;
                                    Quality++;
                                    break;
                                case 2:
                                    Debug.WriteLine("Selected 2");
                                    ESX.ShowNotification("~r~You are a good drug maker, your product is high quality");
                                    Pause = false;
                                    Quality++;
                                    break;
                                case 3:
                                    Debug.WriteLine("Selected 3");
                                    ESX.ShowNotification("~r~That's a bit too much, it's more glass than meth but ok");
                                    Pause = false;
                                    Quality--;
                                    break;
                                default:
                                    ESX.ShowNotification("~o~Do you add some glass pieces to the meth so it looks like you have more of it?");
                                    ESX.ShowNotification("~o~1. Yes!");
                                    ESX.ShowNotification("~o~2. No");
                                    ESX.ShowNotification("~o~3. What if I add meth to glass instead?");
                                    ESX.ShowNotification("~c~Press the number of the option you want to do");
                                    break;
                            }
                        }

                        if (IsPedInAnyVehicle(playerPed, false))
                        {
                            TriggerServerEvent("esx_methcar:make", pos.X, pos.Y, pos.Z);
                            if (!Pause)
                            {
                                Selection = 0;
                                Quality++;
                                Random rnd = new Random();
                                Progress += rnd.Next(1, 2);
                                ESX.ShowNotification($"~r~Meth production: ~g~~h~ {Progress}%");
                            }
                        }
                        else
                        {
                            TriggerEvent("esx_methcar:stop");
                        }
                    }
                    else
                    {
                        TriggerEvent("esx_methcar:stop");
                        Progress = 100;
                        ESX.ShowNotification($"~r~Meth production: ~g~~h~ {Progress}%");
                        ESX.ShowNotification("~g~~h~Production finished");
                        TriggerServerEvent("esx_methcar:finish", Quality);
                        FreezeEntityPosition(LastVehicle, false);
                    }
                }
            }
        }

        private async void VehicleCheck()
        {
            while (true)
            {
                await Delay(1000);
                if (!IsPedInAnyVehicle(GetPlayerPed(-1), false))
                {
                    if (Started)
                    {
                        Started = false;
                        Displayed = false;
                        TriggerEvent("esx_methcar:stop");
                        Debug.WriteLine("Stopped making drugs");
                        FreezeEntityPosition(LastVehicle, false);
                    }
                }
            }
        }

        private async void SelectionInput()
        {
            while (true)
            {
                await Delay(500);
                if (Pause)
                {
                    if (IsControlJustReleased(0, (int)Keys.D1))
                    {
                        Selection = 1;
                        ESX.ShowNotification("~g~Selected option number 1");
                    }
                    else if (IsControlJustReleased(0, (int)Keys.D2))
                    {
                        Selection = 2;
                        ESX.ShowNotification("~g~Selected option number 2");
                    }
                    else if (IsControlJustReleased(0, (int)Keys.D3))
                    {
                        Selection = 3;
                        ESX.ShowNotification("~g~Selected option number 3");
                    }
                }
            }
        }
        #endregion
    }
}
