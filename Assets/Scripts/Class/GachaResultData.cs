using System.Collections.Generic;
using UnityEngine;

public static class GachaResultData
{
    public static List<GachaResultItem> CurrentResults { get; set; } = new List<GachaResultItem>();

    // 清空数据（可选）
    public static void Clear()
    {
        CurrentResults.Clear();
    }
}