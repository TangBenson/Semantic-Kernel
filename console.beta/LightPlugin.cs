using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace console.beta
{
    //控制開關燈的類別
    public class LightPlugin
    {
        //當前燈的狀態
        public bool IsOn { get; set; } = false;

        [KernelFunction]
        [Description("取得燈的狀態")]
        public string GetState()
        {
            return IsOn ? "on" : "off";
        }

        [KernelFunction]
        [Description("改變燈的狀態")]
        public string ChangeState(bool newState)
        {
            this.IsOn = newState;
            var state = GetState();

            // Print the state to the console
            Console.WriteLine($"[Light is now {state}]");

            return state;
        }
    }
}