using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vorpcharacter_cl.Utils;

namespace vorpcharacter_cl
{
    class SelectCharacter : BaseScript
    {
        private int CreatePrompt = -1;
        private int DeletePrompt = -1;

        private int selectedChar = 0;

        private static int ppid = 0;
        private static int mainCamera = -1;
        dynamic myChars = null;
        private static bool isInCharacterSelector = false;
        private int tagId = 0;
        private static bool swappingChar = true;
        private List<string> emotes = new List<string> {
            "KIT_EMOTE_GREET_FANCY_BOW_1",                  //--  0x8186AA35      -2121881035
            "KIT_EMOTE_GREET_FLYING_KISS_1",                //--  0x4F3E0424      1329464356      --new
            "KIT_EMOTE_GREET_GENTLEWAVE_1",                 //--  0x35B5A903      901097731
            "KIT_EMOTE_GREET_GET_OVER_HERE_1",              //--  0x9CA62011      -1666834415
            "KIT_EMOTE_GREET_GLAD_1",                       //--  0x1F3549C4      523585988
            "KIT_EMOTE_GREET_HAND_SHAKE_1",                 //--  0x6A662B8A      1785080714      --new
            "KIT_EMOTE_GREET_HAT_FLICK_1",                  //--  0xE18A99A1      -511010399
            "KIT_EMOTE_GREET_HAT_TIP_1",                    //--  0xA927A00F      -1457020913
            "KIT_EMOTE_GREET_HEY_YOU_1",                    //--  0x3196F0E3      831975651
            "KIT_EMOTE_GREET_OUTPOUR_1",                    //--  0xE68763B3      -427334733      --new
            "KIT_EMOTE_GREET_RESPECTFUL_BOW_1",             //--  0x949C021C      -1801715172
            "KIT_EMOTE_GREET_ROUGH_HOUSING_1",              //--  0xAD277C3D      -1389921219  --new
            "KIT_EMOTE_GREET_SEVEN_1",                      //--  0x3CB5E70E      1018554126
            "KIT_EMOTE_GREET_SUBTLE_WAVE_1",                //--  0xA38D1E64      -1551032732
            "KIT_EMOTE_GREET_TADA_1",                       //--  0xE4746943      -462132925
            "KIT_EMOTE_GREET_THUMBSUP_1",                   //--  0x1960746B      425751659
            "KIT_EMOTE_GREET_TOUGH_1",                      //--  0x700DD5CB      1879954891
            "KIT_EMOTE_GREET_WAVENEAR_1",                   //--  0xEBC75584      -339257980
        };

        public SelectCharacter()
        {
            EventHandlers["vorpcharacter:selectCharacter"] += new Action<dynamic>(LoadCharacters);
            EventHandlers["vorpcharacter:spawnUniqueCharacter"] += new Action<dynamic>(SpawnCharacter);
        }

        private async void SpawnCharacter(dynamic myChar)
        {
            try
            {
                int charIdentifier = int.Parse(myChar[0].charIdentifier.ToString());
                TriggerServerEvent("vorp_CharSelectedCharacter", charIdentifier);

                string json_skin = myChar[0].skin;
                string json_components = myChar[0].components;
                string json_coords = myChar[0].coords;
                JObject jPos = JObject.Parse(json_coords);
                await Delay(400);
                TriggerEvent("vorpcharacter:loadPlayerSkin", json_skin, json_components);
                API.DoScreenFadeOut(500);
                await Delay(400);
                Vector3 playerCoords = new Vector3(jPos["x"].ToObject<float>(), jPos["y"].ToObject<float>(), jPos["z"].ToObject<float>());
                bool isDead = false;
                try
                {
                    isDead = (bool)myChar[0].isDead;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                float heading = jPos["heading"].ToObject<float>();
                TriggerEvent("vorp:initCharacter", playerCoords, heading, isDead);
                await Delay(1000);
                API.DoScreenFadeIn(1000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                API.DoScreenFadeIn(1000);
            }
        }

        public async Task StartAnim()
        {
            Vector3 characterSelectionFinalCoords = new Vector3(GetConfig.Config["CharacterSelectionFinalCoords"][0].ToObject<float>(), GetConfig.Config["CharacterSelectionFinalCoords"][1].ToObject<float>(), GetConfig.Config["CharacterSelectionFinalCoords"][2].ToObject<float>());
            Vector3 CharacterSelectionStartCoords = new Vector3(GetConfig.Config["CharacterSelectionStartCoords"][0].ToObject<float>(), GetConfig.Config["CharacterSelectionStartCoords"][1].ToObject<float>(), GetConfig.Config["CharacterSelectionStartCoords"][2].ToObject<float>());

            uint hashmodel = (uint)API.GetHashKey("mp_male");
            await Miscellanea.LoadModel(hashmodel);
            int character_1 = API.CreatePed(hashmodel, CharacterSelectionStartCoords.X, CharacterSelectionStartCoords.Y, CharacterSelectionStartCoords.Z, 116.70f, false, false, true, true);

            Function.Call((Hash)0x283978A15512B2FE, character_1, true);
            await Delay(1000);
            API.TaskGoToCoordAnyMeans(character_1, characterSelectionFinalCoords.X, characterSelectionFinalCoords.Y, characterSelectionFinalCoords.Z, 0.5f, 0, false, 524419, -1f);
            await Delay(8000);
            API.TaskGoToCoordAnyMeans(character_1, CharacterSelectionStartCoords.X, CharacterSelectionStartCoords.Y, CharacterSelectionStartCoords.Y, 0.5f, 0, false, 524419, -1f);
            await Delay(5000);
            API.DeletePed(ref character_1);
        }

        private void RegisterPrompts()
        {
            //Left
            CreatePrompt = API.PromptRegisterBegin();
            Function.Call((Hash)0xB5352B7494A08258, CreatePrompt, 0x9959A6F0);
            long str_previous = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", GetConfig.Langs["CreateNewChar"]);
            Function.Call((Hash)0x5DD02A8318420DD7, CreatePrompt, str_previous);
            API.PromptSetEnabled(CreatePrompt, 0);
            API.PromptSetVisible(CreatePrompt, 0);
            API.PromptSetHoldMode(CreatePrompt, 1);
            API.PromptRegisterEnd(CreatePrompt);

            //Enter
            DeletePrompt = API.PromptRegisterBegin();
            Function.Call((Hash)0xB5352B7494A08258, DeletePrompt, 0x156F7119);
            long str_select = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", GetConfig.Langs["DeleteChar"]);
            Function.Call((Hash)0x5DD02A8318420DD7, DeletePrompt, str_select);
            API.PromptSetEnabled(DeletePrompt, 0);
            API.PromptSetVisible(DeletePrompt, 0);
            API.PromptSetHoldMode(DeletePrompt, 1);
            API.PromptRegisterEnd(DeletePrompt);

        }

        public async Task MoveToCoords(Vector3 target)
        {
            Vector3 pedCoord = API.GetEntityCoords(ppid, false, true);

            float distance = API.GetDistanceBetweenCoords(pedCoord.X, pedCoord.Y, pedCoord.Z, target.X, target.Y, target.Z, false);
            int counter = 0;
            float lastDistance = 0;
            while (distance > 0.3f && counter < 10)
            {
                API.TaskGoToCoordAnyMeans(ppid, target.X, target.Y, target.Z, 0.8f, 0, false, 524419, -1f);
                await Delay(500);
                while (API.IsPedWalking(ppid)) { await Delay(500); }
                pedCoord = API.GetEntityCoords(ppid, false, true);
                distance = API.GetDistanceBetweenCoords(pedCoord.X, pedCoord.Y, pedCoord.Z, target.X, target.Y, target.Z, false);

                if (lastDistance == distance)
                {
                    counter++;
                }
                else
                {
                    counter = 0;
                }
                lastDistance = distance;
            }
        }

        public async Task StartSwapCharacter()
        {
            swappingChar = true;
            string json_skin = myChars[selectedChar].skin;
            string json_components = myChars[selectedChar].components;

            API.PromptSetEnabled(DeletePrompt, 0);
            API.PromptSetVisible(DeletePrompt, 0);
            API.PromptSetEnabled(CreatePrompt, 0);
            API.PromptSetVisible(CreatePrompt, 0);

            Vector3 characterSelectionFinalCoords = new Vector3(GetConfig.Config["CharacterSelectionFinalCoords"][0].ToObject<float>(), GetConfig.Config["CharacterSelectionFinalCoords"][1].ToObject<float>(), GetConfig.Config["CharacterSelectionFinalCoords"][2].ToObject<float>());
            Vector3 characterSelectionMiddleCoords = new Vector3(GetConfig.Config["CharacterSelectionMiddleCoords"][0].ToObject<float>(), GetConfig.Config["CharacterSelectionMiddleCoords"][1].ToObject<float>(), GetConfig.Config["CharacterSelectionMiddleCoords"][2].ToObject<float>());
            Vector3 characterSelectionWorshipCoords = new Vector3(GetConfig.Config["CharacterSelectionWorshipCoords"][0].ToObject<float>(), GetConfig.Config["CharacterSelectionWorshipCoords"][1].ToObject<float>(), GetConfig.Config["CharacterSelectionWorshipCoords"][2].ToObject<float>());

            if (ppid != 0)
            {
                await MoveToCoords(characterSelectionFinalCoords);
            }

            Function.Call((Hash)0xA0D7CE5F83259663, tagId, "");
            Function.Call((Hash)0x839BFD7D7E49FE09, tagId);
            API.DeletePed(ref ppid);
            await LoadNpcComps(json_skin, json_components);
            await Delay(10);
            await LoadNpcComps(json_skin, json_components);
            tagId = Function.Call<int>((Hash)0x53CB4B502E1C57EA, ppid, $"{GetConfig.Langs["MoneyTag"]}: ~COLOR_WHITE~$" + "~COLOR_REPLAY_GREEN~" + myChars[selectedChar].money, false, false, "", 0);
            Function.Call((Hash)0xA0D7CE5F83259663, tagId, myChars[selectedChar].firstname + " " + myChars[selectedChar].lastname);
            Function.Call((Hash)0x5F57522BC1EB9D9D, tagId, 0);
            await Delay(200);

            await MoveToCoords(characterSelectionMiddleCoords);
            await MoveToCoords(characterSelectionWorshipCoords);

            var emote_category = 3;
            Function.Call((Hash)0xB31A277C1AC7B7FF, ppid, emote_category, 2, API.GetHashKey(emotes[new Random(DateTime.Now.Millisecond).Next(0, emotes.Count - 1)]), 0, 0, 0, 0, 0);
            await Delay(2000);

            API.PromptSetEnabled(DeletePrompt, 1);
            API.PromptSetVisible(DeletePrompt, 1);
            API.PromptSetEnabled(CreatePrompt, 1);
            API.PromptSetVisible(CreatePrompt, 1);
            await Delay(300);
            swappingChar = false;
        }


        public async void LoadCharacters(dynamic myCharacters)
        {
            API.DoScreenFadeOut(100);
            RegisterPrompts();

            isInCharacterSelector = true;
            await Controller();
            await DrawInformation();
            Function.Call(Hash.SET_CLOCK_TIME, 12, 00, 0);
            API.SetClockTime(12, 00, 00);

            await StartSwapCharacter();

            //Arthur Morgan
            API.SetEntityCoords(API.PlayerPedId(), 2546.91f, -1304.16f, 49.1f, false, false, false, false);

            Vector3 camCoors = new Vector3(GetConfig.Config["MainCamCoords"][0].ToObject<float>(), GetConfig.Config["MainCamCoords"][1].ToObject<float>(), GetConfig.Config["MainCamCoords"][2].ToObject<float>());
            Vector4 camRotation = new Vector4(GetConfig.Config["MainCamRotations"][0].ToObject<float>(), GetConfig.Config["MainCamRotations"][1].ToObject<float>(), GetConfig.Config["MainCamRotations"][2].ToObject<float>(), GetConfig.Config["MainCamRotations"][3].ToObject<float>());
            mainCamera = API.CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", camCoors.X, camCoors.Y, camCoors.Z, camRotation.X, camRotation.Y, camRotation.Z, camRotation.W, false, 0);

            API.SetCamActive(mainCamera, true);

            API.RenderScriptCams(true, true, 1000, true, true, 0);

            await Delay(15000);

            await StartSwapCharacter();

            API.DoScreenFadeIn(1000);
        }

        public async Task DrawInformation()
        {
            Vector3 characterSelectionFinalCoords = new Vector3(GetConfig.Config["CharacterSelectionFinalCoords"][0].ToObject<float>(), GetConfig.Config["CharacterSelectionFinalCoords"][1].ToObject<float>(), GetConfig.Config["CharacterSelectionFinalCoords"][2].ToObject<float>());
            Debug.WriteLine($"{characterSelectionFinalCoords.X}, {characterSelectionFinalCoords.Y}, {characterSelectionFinalCoords.Z}");
            while (isInCharacterSelector)
            {
                //Final Point for Light
                API.DrawLightWithRange(characterSelectionFinalCoords.X, characterSelectionFinalCoords.Y, characterSelectionFinalCoords.Z, 255, 255, 255, 8.0f, 250.0f);
                await Utils.Miscellanea.DrawTxt(GetConfig.Langs["PressSelectInfo"], 0.5f, 0.90f, 0.75f, 0.70f, 255, 255, 255, 255, true, false);
                await Delay(0);
            }
        }

        public async Task CharSelect()
        {
            try
            {

                int charIdentifier = int.Parse(myChars[selectedChar].charIdentifier.ToString());
                TriggerServerEvent("vorp_CharSelectedCharacter", charIdentifier);

                API.PromptSetEnabled(DeletePrompt, 0);
                API.PromptSetVisible(DeletePrompt, 0);
                API.PromptSetEnabled(CreatePrompt, 0);
                API.PromptSetVisible(CreatePrompt, 0);
                API.DeletePed(ref ppid);

                string json_skin = myChars[selectedChar].skin;
                string json_components = myChars[selectedChar].components;
                string json_coords = myChars[selectedChar].coords;
                JObject jPos = JObject.Parse(json_coords);

                await LoadNpcComps(json_skin, json_components);
                await Delay(10);
                await LoadNpcComps(json_skin, json_components);

                TriggerEvent("vorpcharacter:loadPlayerSkin", json_skin, json_components);
                API.DoScreenFadeOut(100); // It is necessary so that the world has time to load and the player does not have to see empty textures
                await Delay(800);
                API.SetCamActive(mainCamera, false);
                API.DestroyCam(mainCamera, true);
                API.RenderScriptCams(true, true, 1000, true, true, 0);
                Vector3 playerCoords = new Vector3(jPos["x"].ToObject<float>(), jPos["y"].ToObject<float>(), jPos["z"].ToObject<float>());
                bool isDead = false;
                try
                {
                    isDead = (bool)myChars[selectedChar].isDead;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                float heading = jPos["heading"].ToObject<float>();
                TriggerEvent("vorp:initCharacter", playerCoords, heading, isDead);
                // It is necessary so that the world has time to load and the player does not have to see empty textures
                await Delay(15000);
                API.DoScreenFadeIn(1000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                API.DoScreenFadeIn(1000);
            }
        }

        public async Task Controller()
        {
            while (isInCharacterSelector)
            {
                if (API.IsControlJustPressed(0, 0xDEB34313) && !swappingChar)
                {
                    if (selectedChar == myChars.Count - 1)
                    {
                        selectedChar = 0;
                    }
                    else
                    {
                        selectedChar += 1;
                    }
                    await StartSwapCharacter();
                }

                if (API.IsControlJustPressed(0, 0xA65EBAB4) && !swappingChar)
                {
                    if (selectedChar == 0)
                    {
                        selectedChar = myChars.Count - 1;
                    }
                    else
                    {
                        selectedChar -= 1;
                    }
                    await StartSwapCharacter();
                }

                if (API.IsControlJustPressed(0, 0xC7B5340A) && !swappingChar)
                {
                    CharSelect();
                    isInCharacterSelector = false;
                    await Delay(200);
                }

                if (API.PromptHasHoldModeCompleted(CreatePrompt) && !swappingChar)
                {
                    if (myChars.Count < vorpcharacter_cl.MaxCharacters)
                    {
                        API.DoScreenFadeOut(1000);
                        TriggerEvent("vorpcharacter:createCharacter");
                        API.PromptSetEnabled(DeletePrompt, 0);
                        API.PromptSetVisible(DeletePrompt, 0);
                        API.PromptSetEnabled(CreatePrompt, 0);
                        API.PromptSetVisible(CreatePrompt, 0);
                        API.DeletePed(ref ppid);
                        isInCharacterSelector = false;
                        await Delay(7000);
                        API.DoScreenFadeIn(1000);
                    }
                    else
                    {
                        vorpcharacter_cl.CORE.displayRightTip(GetConfig.Langs["CharactersFull"], 4000);
                        await Delay(1000);
                    }
                }

                if (API.PromptHasHoldModeCompleted(DeletePrompt) && !swappingChar)
                {
                    TriggerServerEvent("vorp_DeleteCharacter", (int)myChars[selectedChar].charIdentifier);
                    if (myChars.Count <= 1)
                    {
                        API.DoScreenFadeOut(1000);
                        TriggerEvent("vorpcharacter:createCharacter");
                        API.PromptSetEnabled(DeletePrompt, 0);
                        API.PromptSetVisible(DeletePrompt, 0);
                        API.PromptSetEnabled(CreatePrompt, 0);
                        API.PromptSetVisible(CreatePrompt, 0);
                        API.DeletePed(ref ppid);
                        isInCharacterSelector = false;
                        await Delay(7000);
                        API.DoScreenFadeIn(1000);
                    }
                    else
                    {
                        myChars.RemoveAt(selectedChar);

                        if (selectedChar == 0)
                        {
                            selectedChar = myChars.Count - 1;
                        }
                        else
                        {
                            selectedChar -= 1;
                        }

                        await StartSwapCharacter();
                    }
                }

                await Delay(0);
            }
            
        }

        public async Task LoadNpcComps(string skin_json, string cloths_json)
        {
            JObject jskin = JObject.Parse(skin_json);
            JObject jcomp = JObject.Parse(cloths_json);

            Dictionary<string, string> skin = new Dictionary<string, string>();

            foreach (var s in jskin)
            {
                skin[s.Key] = s.Value.ToString();
            }

            Dictionary<string, uint> cloths = new Dictionary<string, uint>();

            foreach (var s in jcomp)
            {
                cloths[s.Key] = LoadPlayer.ConvertValue(s.Value.ToString());
            }

            uint model_hash = (uint)API.GetHashKey(skin["sex"]);
            await Utils.Miscellanea.LoadModel(model_hash);
            await Delay(10);

            Vector3 characterSelectionStartCoords = new Vector3(GetConfig.Config["CharacterSelectionStartCoords"][0].ToObject<float>(), GetConfig.Config["CharacterSelectionStartCoords"][1].ToObject<float>(), GetConfig.Config["CharacterSelectionStartCoords"][2].ToObject<float>());
            //ppid = API.CreatePed(model_hash, 1701.316f, 1512.134f, 146.87f, 116.70f, false, false, true, true); //Inside house
            ppid = API.CreatePed(model_hash, characterSelectionStartCoords.X, characterSelectionStartCoords.Y, characterSelectionStartCoords.Z, 116.70f, false, false, true, true); //Inside house
            API.DisablePedPainAudio(ppid, true);
            await Delay(300);

            Function.Call((Hash)0xCC8CA3E88256E58F, ppid, 0, 1, 1, 1, false);

            //PreLoad TextureFace
            if (skin["sex"].ToString().Equals("mp_male"))
            {
                CreateCharacter.texture_types["albedo"] = int.Parse(skin["albedo"]);
                CreateCharacter.texture_types["normal"] = API.GetHashKey("mp_head_mr1_000_nm");
                CreateCharacter.texture_types["material"] = 0x7FC5B1E1;
                CreateCharacter.texture_types["color_type"] = 1;
                CreateCharacter.texture_types["texture_opacity"] = 1.0f;
                CreateCharacter.texture_types["unk_arg"] = 0;
            }
            else
            {
                CreateCharacter.texture_types["albedo"] = int.Parse(skin["albedo"]);
                CreateCharacter.texture_types["normal"] = API.GetHashKey("head_fr1_mp_002_nm");
                CreateCharacter.texture_types["material"] = 0x7FC5B1E1;
                CreateCharacter.texture_types["color_type"] = 1;
                CreateCharacter.texture_types["texture_opacity"] = 1.0f;
                CreateCharacter.texture_types["unk_arg"] = 0;
            }
            //End
            CreateCharacter.ApplyDefaultSkinCanaryEdition(ppid);

            //LoadSkin
            Function.Call((Hash)0xD3A7B003ED343FD9, ppid, LoadPlayer.ConvertValue(skin["HeadType"]), true, true, true);
            Function.Call((Hash)0xCC8CA3E88256E58F, ppid, 0, 1, 1, 1, false);
            Function.Call((Hash)0xD3A7B003ED343FD9, ppid, LoadPlayer.ConvertValue(skin["BodyType"]), true, true, true);
            Function.Call((Hash)0xCC8CA3E88256E58F, ppid, 0, 1, 1, 1, false);
            Function.Call((Hash)0xD3A7B003ED343FD9, ppid, LoadPlayer.ConvertValue(skin["LegsType"]), true, true, true);
            Function.Call((Hash)0xCC8CA3E88256E58F, ppid, 0, 1, 1, 1, false);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x84D6, float.Parse(skin["HeadSize"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x3303, float.Parse(skin["EyeBrowH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x2FF9, float.Parse(skin["EyeBrowW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x4AD1, float.Parse(skin["EyeBrowD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xC04F, float.Parse(skin["EarsH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xB6CE, float.Parse(skin["EarsW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x2844, float.Parse(skin["EarsD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xED30, float.Parse(skin["EarsL"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x8B2B, float.Parse(skin["EyeLidH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x1B6B, float.Parse(skin["EyeLidW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xEE44, float.Parse(skin["EyeD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xD266, float.Parse(skin["EyeAng"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xA54E, float.Parse(skin["EyeDis"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xDDFB, float.Parse(skin["EyeH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x6E7F, float.Parse(skin["NoseW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x3471, float.Parse(skin["NoseS"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x03F5, float.Parse(skin["NoseH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x34B1, float.Parse(skin["NoseAng"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xF156, float.Parse(skin["NoseC"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x561E, float.Parse(skin["NoseDis"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x6A0B, float.Parse(skin["CheekBonesH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xABCF, float.Parse(skin["CheekBonesW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x358D, float.Parse(skin["CheekBonesD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xF065, float.Parse(skin["MouthW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xAA69, float.Parse(skin["MouthD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x7AC3, float.Parse(skin["MouthX"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x410D, float.Parse(skin["MouthY"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x1A00, float.Parse(skin["ULiphH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x91C1, float.Parse(skin["ULiphW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xC375, float.Parse(skin["ULiphD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xBB4D, float.Parse(skin["LLiphH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xB0B0, float.Parse(skin["LLiphW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x5D16, float.Parse(skin["LLiphD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x8D0A, float.Parse(skin["JawH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xEBAE, float.Parse(skin["JawW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x1DF6, float.Parse(skin["JawD"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0x3C0F, float.Parse(skin["ChinH"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xC3B2, float.Parse(skin["ChinW"]));
            await Delay(10);

            Function.Call((Hash)0x5653AB26C82938CF, ppid, 0xE323, float.Parse(skin["ChinD"]));
            await Delay(10);

            Function.Call((Hash)0xD3A7B003ED343FD9, ppid, LoadPlayer.ConvertValue(skin["Eyes"]), true, true, true);
            await Delay(10);

            Function.Call((Hash)0x1902C4CFCC5BE57C, ppid, LoadPlayer.ConvertValue(skin["Body"]));
            await Delay(10);

            Function.Call((Hash)0x1902C4CFCC5BE57C, ppid, LoadPlayer.ConvertValue(skin["Waist"]));

            Function.Call((Hash)0xCC8CA3E88256E58F, ppid, 0, 1, 1, 1, false);

            await Delay(500);
            SetPlayerComponent(skin["sex"], 0x9925C067, "Hat", cloths);
            SetPlayerComponent(skin["sex"], 0x5E47CA6, "EyeWear", cloths);
            SetPlayerComponent(skin["sex"], 0x7505EF42, "Mask", cloths);
            SetPlayerComponent(skin["sex"], 0x5FC29285, "NeckWear", cloths);
            SetPlayerComponent(skin["sex"], 0x877A2CF7, "Suspender", cloths);
            SetPlayerComponent(skin["sex"], 0x485EE834, "Vest", cloths);
            SetPlayerComponent(skin["sex"], 0xE06D30CE, "Coat", cloths);
            SetPlayerComponent(skin["sex"], 0x0662AC34, "CoatClosed", cloths);
            SetPlayerComponent(skin["sex"], 0x2026C46D, "Shirt", cloths);
            SetPlayerComponent(skin["sex"], 0x7A96FACA, "NeckTies", cloths);
            SetPlayerComponent(skin["sex"], 0xAF14310B, "Poncho", cloths);
            SetPlayerComponent(skin["sex"], 0x3C1A74CD, "Cloak", cloths);
            SetPlayerComponent(skin["sex"], 0xEABE0032, "Glove", cloths);
            SetPlayerComponent(skin["sex"], 0x7A6BBD0B, "RingRh", cloths);
            SetPlayerComponent(skin["sex"], 0xF16A1D23, "RingLh", cloths);
            SetPlayerComponent(skin["sex"], 0x7BC10759, "Bracelet", cloths);
            SetPlayerComponent(skin["sex"], 0x9B2C8B89, "Gunbelt", cloths);
            SetPlayerComponent(skin["sex"], 0xA6D134C6, "Belt", cloths);
            SetPlayerComponent(skin["sex"], 0xFAE9107F, "Buckle", cloths);
            SetPlayerComponent(skin["sex"], 0xB6B6122D, "Holster", cloths);
            if (cloths["Skirt"] != -1) // Prevents both Pant & Skirt in female ped.
            {
                SetPlayerComponent(skin["sex"], 0x1D4C528A, "Pant", cloths);
            }
            SetPlayerComponent(skin["sex"], 0xA0E3AB7F, "Skirt", cloths);
            SetPlayerComponent(skin["sex"], 0x3107499B, "Chap", cloths);
            SetPlayerComponent(skin["sex"], 0x777EC6EF, "Boots", cloths);
            SetPlayerComponent(skin["sex"], 0x18729F39, "Spurs", cloths);
            SetPlayerComponent(skin["sex"], 0x514ADCEA, "Spats", cloths);
            SetPlayerComponent(skin["sex"], 0xF1542D11, "GunbeltAccs", cloths);
            SetPlayerComponent(skin["sex"], 0x91CE9B20, "Gauntlets", cloths);
            SetPlayerComponent(skin["sex"], 0x83887E88, "Loadouts", cloths);
            SetPlayerComponent(skin["sex"], 0x79D7DF96, "Accessories", cloths);
            SetPlayerComponent(skin["sex"], 0x94504D26, "Satchels", cloths);

            Function.Call((Hash)0xCC8CA3E88256E58F, ppid, 0, 1, 1, 1, false);

            await Delay(1000);
            Function.Call((Hash)0x59BD177A1A48600A, ppid, 0xF8016BCA);
            Function.Call((Hash)0xD3A7B003ED343FD9, ppid, LoadPlayer.ConvertValue(skin["Beard"]), true, true, true);
            Function.Call((Hash)0xD3A7B003ED343FD9, ppid, LoadPlayer.ConvertValue(skin["Teeth"]), true, true, true);
            Function.Call((Hash)0xD3A7B003ED343FD9, ppid, LoadPlayer.ConvertValue(skin["Hair"]), true, true, true);

            Function.Call((Hash)0xCC8CA3E88256E58F, ppid, 0, 1, 1, 1, false);

        }

        public static void SetPlayerComponent(string model, uint category, string component, Dictionary<string, uint> cloths)
        {
            if (model == "mp_male")
            {
                if (cloths[component] != -1)
                {
                    Function.Call((Hash)0x59BD177A1A48600A, ppid, category);
                    Function.Call((Hash)0xD3A7B003ED343FD9, ppid, cloths[component], true, true, false);
                }
            }
            else
            {
                Function.Call((Hash)0x59BD177A1A48600A, ppid, category);
                Function.Call((Hash)0xD3A7B003ED343FD9, ppid, cloths[component], true, true, true);
            }

            //Function.Call((Hash)0xCC8CA3E88256E58F, pPID, 0, 1, 1, 1, false);
        }

        public static async Task DrawTxt3D(float x, float y, float z, string text)
        {
            float _x = 0.0F;
            float _y = 0.0F;
            //Debug.WriteLine(position.X.ToString());
            API.GetScreenCoordFromWorldCoord(x, y, z, ref _x, ref _y);
            API.SetTextScale(0.35F, 0.35F);
            API.SetTextFontForCurrentCommand(1);
            API.SetTextColor(255, 255, 255, 215);
            long str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", text);
            Function.Call((Hash)0xBE5261939FBECB8C, 1);
            Function.Call((Hash)0xD79334A4BB99BAD1, str, _x, _y);
            //float factor = text.Length / 150.0F;
            //Function.Call((Hash)0xC9884ECADE94CB34, "generic_textures", "hud_menu_4a", _x, _y + 0.0125F, 0.015F + factor, 0.03F, 0.1F, 100, 1, 1, 190, 0);
        }

    }
}
