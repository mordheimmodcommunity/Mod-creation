using System;
using System.Collections.Generic;
using UnityEngine;

public class KGFGUIRenderer : KGFObject, KGFIValidator
{
    private List<KGFIGui2D> itsGUIs = new List<KGFIGui2D>();

    protected override void KGFAwake()
    {
        itsGUIs = KGFAccessor.GetObjects<KGFIGui2D>();
        KGFAccessor.RegisterAddEvent<KGFIGui2D>(OnRegisterKGFIGui2D);
        KGFAccessor.RegisterRemoveEvent<KGFIGui2D>(OnUnregisterKGFIGui2D);
    }

    private void OnRegisterKGFIGui2D(object theSender, EventArgs theArgs)
    {
        KGFAccessor.KGFAccessorEventargs kGFAccessorEventargs = theArgs as KGFAccessor.KGFAccessorEventargs;
        if (kGFAccessorEventargs != null)
        {
            KGFIGui2D kGFIGui2D = kGFAccessorEventargs.GetObject() as KGFIGui2D;
            if (kGFIGui2D != null)
            {
                itsGUIs.Add(kGFIGui2D);
                itsGUIs.Sort(CompareKGFIGui2D);
            }
        }
    }

    private void OnUnregisterKGFIGui2D(object theSender, EventArgs theArgs)
    {
        KGFAccessor.KGFAccessorEventargs kGFAccessorEventargs = theArgs as KGFAccessor.KGFAccessorEventargs;
        if (kGFAccessorEventargs != null)
        {
            KGFIGui2D kGFIGui2D = kGFAccessorEventargs.GetObject() as KGFIGui2D;
            if (kGFIGui2D != null && itsGUIs.Contains(kGFIGui2D))
            {
                itsGUIs.Remove(kGFIGui2D);
            }
        }
    }

    private int CompareKGFIGui2D(KGFIGui2D theGui1, KGFIGui2D theGui2)
    {
        return theGui1.GetLayer().CompareTo(theGui2.GetLayer());
    }

    protected void OnGUI()
    {
        float scaleFactor2D = KGFScreen.GetScaleFactor2D();
        GUIUtility.ScaleAroundPivot(new Vector2(scaleFactor2D, scaleFactor2D), Vector2.zero);
        foreach (KGFIGui2D itsGUI in itsGUIs)
        {
            itsGUI.RenderGUI();
        }
        GUI.matrix = Matrix4x4.identity;
    }

    public virtual KGFMessageList Validate()
    {
        return new KGFMessageList();
    }
}
