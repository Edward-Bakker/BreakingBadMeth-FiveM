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

        public Main()
        {
            TriggerEvent("esx:getSharedObject", new object[] { new Action<dynamic>(esx =>
            {
                ESX = esx;
            })});

            EventHandlers["esx_methcar:startprod"] += new Action(StartProduction);
            EventHandlers["esx_methcar:startcooking"] += new Action<Vehicle>(StartCooking);
            EventHandlers["esx_methcar:stop"] += new Action(StopProduction);
            EventHandlers["esx_methcar:stopfreeze"] += new Action<int>(StopFreeze);
            EventHandlers["esx_methcar:blowup"] += new Action<float, float, float>(BlowUpVehicle);
            EventHandlers["esx_methcar:smoke"] += new Action<float, float, float>(VehicleSmoke);
            EventHandlers["esx_methcar:drugged"] += new Action(Drugged);
            EventHandlers["esx_methcar:vehiclecheck"] += new Action(VehicleCheck);

            GetLastInput();
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

        private async void StartCooking(Vehicle currentVehicle)
        {
            int playerPed = GetPlayerPed(-1);
            Vector3 pos = GetEntityCoords(playerPed, true);
            CurrentVehicle = GetVehiclePedIsUsing(playerPed);
            LastVehicle = GetVehiclePedIsUsing(playerPed);
            Random rnd = new Random();
            Progress = 0;
            Quality = 0;

            int vehicle = GetVehiclePedIsIn(playerPed, false);

            DisplayHelpText("Press ~INPUT_THROW_GRENADE~ to start making meth.");

            while (true)
            {
                await Delay(500);

                if (vehicle >= 1 && GetPedInVehicleSeat(vehicle, -1) == playerPed)
                {
                    if (IsControlJustReleased(0, (int) Keys.G))
                    {
                        if (pos.Y >= 3500)
                        {
                            if (IsVehicleSeatFree(CurrentVehicle, 3))
                            {
                                TriggerServerEvent("esx_methcar:start");
                                Pause = false;
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

                if (Started)
                {
                    if (Progress < 100)
                    {
                        await Delay(6000);
                        if (!Pause)
                        {
                            Progress++;
                            ESX.ShowNotification($"~r~Meth production: ~g~~h~ {Progress} % ");
                        }

                        switch(Progress)
                        {
                            case 20:
                                //Implement first event
                                break;
                            case 30:
                                //Implement second event
                                break;
                            case 40:
                                //Implement third event
                                break;
                            case 45:
                                //Implement fourth event
                                break;
                            case 50:
                                //Implement fifth event
                                break;
                            case 55:
                                //Implement sixth event
                                break;
                            case 60:
                                //Implement seventh event
                                break;
                            case 65:
                                //Implement eigth event
                                break;
                            case 70:
                                //Implement ninth event
                                break;
                            case 75:
                                //Implement tenth event
                                break;
                            case 80:
                                //Implement eleventh event
                                break;
                            case 90:
                                //Implement twelfth event
                                break;
                        }

                        if (IsPedInAnyVehicle(playerPed, false))
                        {
                            TriggerServerEvent("esx_methcar:make", pos.X, pos.Y, pos.Z);
                            if (!Pause)
                            {
                                Selection = 0;
                                Quality++;
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
                        ESX.ShowNotification("~g~~h~Production finished");
                        TriggerServerEvent("esx_methcar:finish", Quality);
                        FreezeEntityPosition(LastVehicle, false);
                    }
                }
            }
        }

        private void VehicleCheck()
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

        private async void GetLastInput()
        {
            while (true)
            {
                await Delay(1000);
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
    }
}
