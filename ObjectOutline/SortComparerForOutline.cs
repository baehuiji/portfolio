//아이템의 아웃라인 생성을 위해 인덱스를 정렬합니다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortComparerForOutline : IComparer<EasyOutlineSystem>
{
    public int Compare(EasyOutlineSystem x, EasyOutlineSystem y)
    {
        if (x.index > y.index)
            return 1;

        if (x.index == y.index)
            return 0;

        if (x.index < y.index)
            return -1;

        return 0;
    }
}
