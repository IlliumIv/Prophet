using System.Windows.Forms;
using System.Collections.Generic;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Attributes;
using SharpDX;

namespace Prophet
{
    public class ProphetSettings : ISettings
    {

        public ProphetSettings()
        {
            Enable = new ToggleNode(false);

            ColorGood = new ColorBGRA(0, 255, 0, 255);

            ColorTrash = new ColorBGRA(0, 0, 255, 255);
            
            #region PoeNinjaUnique

            Update = new ToggleNode(false);

            List<string> listLeagues = new List<string>()
            {
                "Temp SC", "Temp HC"
            };
            League = new ListNode();
            League.SetListValues(listLeagues);

            ChaosValueGood = new RangeNode<int>(2, 0, 50);
            
            ChaosValueTrash = new RangeNode<int>(2, 0, 50);
            
            #endregion
        }

        [Menu("Menu Settings", 100)]
        public EmptyNode Settings { get; set; }

        [Menu("Enable", parentIndex = 100, Tooltip = "Enable Prophet")]
        public ToggleNode Enable { get; set; } 

        [Menu("Color good prophecy", parentIndex = 100, Tooltip = "Display color of border good prophecy")]
        public ColorNode ColorGood { get; set; }

        [Menu("Color trash prophecy", parentIndex = 100, Tooltip = "Display color of border trash prophecy")]
        public ColorNode ColorTrash { get; set; }

        #region PoeNinja

        [Menu("Parsing Poe Ninja Setting", 99)] 
        public EmptyNode PoeNinjaUnique { get; set; }

        [Menu("Enable", parentIndex = 99, Tooltip = "Parsing poe ninja for autovendor trash uniques items")]
        public ToggleNode Update { get; set; }
        
        [Menu("League", parentIndex = 99, Tooltip = "Choose league")]
        public ListNode League { get; set; }
        
        [Menu("Chaos", parentIndex = 99, Tooltip = "Set min chaos value for good prophecy")]
        public RangeNode<int> ChaosValueGood { get; set; }
        
        [Menu("Chaos", parentIndex = 99, Tooltip = "Set max chaos value for trash prophecy")]
        public RangeNode<int> ChaosValueTrash { get; set; }
        
        [Menu("Refresh Data", parentIndex = 99, Tooltip = "Press to update after configuring chaos values. Do not spam.")]
        public ButtonNode Refresh { get; set; } = new ButtonNode();
        #endregion

    }
}
