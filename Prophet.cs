using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SharpDX;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore;
using Newtonsoft.Json.Linq;

namespace Prophet
{


    public class Prophet : BaseSettingsPlugin<ProphetSettings>
    {
        private List<string> PropheciesListGood;
        private List<string> PropheciesListTrash;
        private Vector2 _windowOffset;
        private IngameState _ingameState;

        public override bool Initialise()
        {
            _ingameState = GameController.Game.IngameState;
            _windowOffset = GameController.Window.GetWindowRectangle().TopLeft;

            RefreshData();
            Settings.Refresh.OnPressed += () => { RefreshData(); };
            Settings.League.OnValueSelectedPre += () => { RefreshData(); };
            return true;
        }
        
        private void RefreshData() {
            if (Settings.Update.Value) {
                ParsingPoeNinja();
            }
            else {
                ReadProphecies();
            }
        }

        public override void Render()
        {
            if (!_ingameState.IngameUi.OpenRightPanel.IsVisible)
                return;

            GotProphecies();
        }


        private void GotProphecies()
        {
            //
            var ProphecyPanel = _ingameState.IngameUi.OpenLeftPanel.GetChildAtIndex(2)?.GetChildAtIndex(0)?.GetChildAtIndex(1)?.GetChildAtIndex(1)?.GetChildAtIndex(40)?.GetChildAtIndex(0)?.GetChildAtIndex(0);

            if ((ProphecyPanel == null) || (!ProphecyPanel.IsVisible)) 
                return;

            if (ProphecyPanel.ChildCount <= 0) 
                return;

            var foundprophsGood = new List<RectangleF>();
            var foundprophsTrash = new List<RectangleF>();

            foreach (Element element in ProphecyPanel.Children)
            {
                if (element == null)
                    continue;

                var textelement = element.GetChildAtIndex(0).GetChildAtIndex(1);
                var text = element.GetChildAtIndex(0).GetChildAtIndex(1).Text;

                if (PropheciesListGood.Contains(text))
                {
                    var drawRect = textelement.GetClientRect();
                    drawRect.Left -= 5;
                    drawRect.Right += 5;
                    drawRect.Top -= 5;
                    drawRect.Bottom += 5;
                    drawRect.X -= 5;
                    drawRect.Y -= 5;
                    foundprophsGood.Add(drawRect);
                }

                if (PropheciesListTrash.Contains(text))
                {
                    var drawRect = textelement.GetClientRect();
                    drawRect.Left -= 5;
                    drawRect.Right += 5;
                    drawRect.Top -= 5;
                    drawRect.Bottom += 5;
                    drawRect.X -= 5;
                    drawRect.Y -= 5;
                    foundprophsTrash.Add(drawRect);
                }
            }

            if (foundprophsGood.Count > 0)
            {
                DrawFrame(foundprophsGood, Settings.ColorGood);
            }

            if (foundprophsTrash.Count > 0)
            {
                DrawFrame(foundprophsTrash, Settings.ColorTrash);
            }

        }

        private void DrawFrame(List<RectangleF> Prophs, Color color)
        {
            foreach (var position in Prophs)
            {
                RectangleF border = new RectangleF { X = position.X + 8, Y = position.Y + 8, Width = position.Width - 6, Height = position.Height - 6 };
                Graphics.DrawFrame(border, color, 3);
            }
        }

        private void ReadProphecies()
        {
            PropheciesListGood = new List<string>();
            PropheciesListTrash = new List<string>();

            string pathPropGood = Path.Combine( DirectoryFullName ,"prophecies_good.txt");
            string pathPropTrash = Path.Combine( DirectoryFullName , "prophecies_bad.txt");

            CheckConfig(pathPropGood);

            using (StreamReader reader = new StreamReader(pathPropGood))
            {
                string text = reader.ReadToEnd();

                PropheciesListGood = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                reader.Close();
            }

            CheckConfig(pathPropTrash);

            using (StreamReader reader = new StreamReader(pathPropTrash))
            {
                string text = reader.ReadToEnd();

                PropheciesListTrash = text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                reader.Close();
            }
        }

        private void ParsingPoeNinja()
        {
            PropheciesListGood = new List<string>();
            
            PropheciesListTrash = new List<string>();
            
            if (!Settings.Update.Value)
                return;

            #region Parsing

            List<string> uniquesUrls;
            
            switch (Settings.League.Value)
            {
                case "Temp SC" : 
                    uniquesUrls = new List<string>()
                    {
                        @"https://poe.ninja/api/data/itemoverview?league=Ultimatum&type=Prophecy&language=en",
                    };
                    break;
                
                case "Temp HC" : 
                    uniquesUrls = new List<string>()
                    {
                        @"https://poe.ninja/api/data/itemoverview?league=Hardcore%20Ultimatum&type=Prophecy&language=en",
                    };
                    break;
                
                default:
                    uniquesUrls = new List<string>();
                    break;
            }
            
            var resultGood = new HashSet<string>();
            
            var resultTrash = new HashSet<string>();

            foreach (var url in uniquesUrls)
            {
                using (var wc = new WebClient())
                {
                    var json = wc.DownloadString(url);
                    var o = JObject.Parse(json);
                    foreach (var line in o?["lines"])
                    {
                        if (float.TryParse((string) line?["chaosValue"], out var chaosValue))
                        {
                            if (chaosValue >= Settings.ChaosValueGood.Value)
                            {
                                resultGood.Add((string) line?["name"]);
                            }
                            
                            if (chaosValue <= Settings.ChaosValueTrash.Value)
                            {
                                resultTrash.Add((string) line?["name"]);
                            }
                        }
                    }
                }
            }

            var result2G = resultGood.ToList();
            result2G.Sort();

            PropheciesListGood = result2G;
            
            var result2T = resultTrash.ToList();
            result2T.Sort();
            
            PropheciesListTrash = result2T;

            #endregion
        }
        
        private void CheckConfig(string path)
        {
            if (File.Exists(path)) return;

            string text = "";

            using (StreamWriter streamWriter = new StreamWriter(path, true))
            {
                streamWriter.Write(text);
                streamWriter.Close();
            }
        }

    }
}
