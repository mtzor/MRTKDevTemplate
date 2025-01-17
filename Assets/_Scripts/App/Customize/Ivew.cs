using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;

public interface IView
{
    bool IsComplete { get; set; } // Whether the view is shared
    bool IsShared { get; } // Whether the view is shared
    bool IsInCompareMode { get; set; } // Whether the view is in compare mode
    Transform GetCurrentItem();// The current item being shown in the interface
    void ResetCurrentIndex();
    void SetItems(List<Transform> items);
    void NextItem(); // Navigate to the next item
    void PreviousItem(); // Navigate to the previous item
    void ShowCurrentItem(); // Display the current item in the interface
    void CompareViewConvert();//change view from and to compare mode

    void ReportSharedViewState(bool state);
    void DestroyCurrentItem();
    Task FinalizeChoice();//choose layout
    int SelectedIndex();

    List<ulong> SharedClients();
    void SetSharedItemsForClients(int index);
    Transform GetLayoutContainer();
}
