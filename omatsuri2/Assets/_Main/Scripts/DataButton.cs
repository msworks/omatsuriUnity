using UnityEngine;
using System.Collections;

public class DataButton : MonoBehaviour {

    public void OnClick()
    {
        History.Instance.ShiftDisplayHistory();
    }

}
