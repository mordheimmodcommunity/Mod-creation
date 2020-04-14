# Feature: Mirage popup

### MainMenuStartView.cs

```csharp
  public override void Awake()
  {
    base.Awake();
    // ADD LINES - START
    base.StateMachine.ConfirmPopup.Show("menu_warning","M I R A G E", 
    new Action<bool> (this.OnPopup2), false, false);
    // ADD LINES - END
    this.btnContinue.onAction.AddListener(new UnityAction(this.OnContinueCampaign));
    ...code...
  }

  // ADD METHOD
  private void OnPopup2(bool confirm)
  {
    Color yellow = Color.yellow;
    this.ismirage = true;
    this.welcomeDesc.color = yellow;
    this.welcomeDesc.text = "This is the M I R A G E mod v1.0. Start an Exhibition game and select 'Marked for Death' objectives.\nGreetings from Steiner.;
  }
```
### ConfirmationPopupView.cs

```csharp
  public virtual void Show(string titleId, string textId, Action<bool> callback, 
  bool hideButtons = false, bool hideCancel = false)
  {
    ...code..
    if (!string.IsNullOrEmpty(textId))
    {
    this.text.text = PandoraSingleton<LocalizationManager>.Instance.GetStringById(textId);
    }
    // ADD LINES - START
    if (textId == "M I R A G E")
    {
      this.text.text = " This is the M I R A G E  MOD v1.01";
    }
    // ADD LINES - END
    ...code...
  }
```

`--> Improvement ? We need a better loading screen and a Picture --> like seen in the Tutorials`
