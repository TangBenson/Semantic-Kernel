using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace sk.core.Plugins.HolidayPlugin;

internal class HolidayPlugin
{
    [KernelFunction("CheckLogin")]
    [Description("驗證登入身分")]
    public string Login(int id, string pvvd)
    {
        if(id == 80345 && pvvd == "123")
        {
            return "80345";
        }
        else if(id == 85987 && pvvd != "456")
        {
            return "85987";
        }
        else
        {
            return "無此帳號";
        }
    }
    [KernelFunction("GetHoliday")]
    [Description("查詢假期")]
    public int CheckHoliday(int id)
    {
        if(id == 80345)
        {
            return 4;
        }
        else if(id == 85987)
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }
}