# How to create your first mod

- [Create your own mod](https://github.com/mordheimmodcommunity/create-mod/blob/master/README.md#create-your-own-mod)

  - [Install required tools](https://github.com/mordheimmodcommunity/create-mod/blob/master/README.md#install-tools)

  - [Edit assemblies .dll files](https://github.com/mordheimmodcommunity/create-mod/blob/master/README.md#modify-assemblies-with-dnspy)
  
  - Edit database files ( TODO )
  
  - Edit assets files ( TODO )
  
- Share your mod with the community ( TODO )

- Add your mod to the launcher ( TODO )

## Create your mod

### Install tools

[.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer) dependency for dnSpy

[dnSpy](https://github.com/0xd4d/dnSpy/releases/download/v6.1.4/dnSpy-net472.zip) to edit .dll files

[DBBrowser](https://dbeaver.io/files/dbeaver-ce-latest-x86_64-setup.exe) to edit database files

TODO: tools for editing assets files

### Modify assemblies with dnSpy

The game assemblies files are located by default at :
`C:\Program Files (x86)\Steam\SteamApps\common\mordheim\mordheim_Data\Managed`

The file we are interested in is `Assembly-CSharp.dll` which contain most of the game code.

So before starting to edit it with dnSpy, copy the file somewhere so you can revert to the base game without having to repair or reinstall Mordheim.

Start dnSpy, to open a file either use `file` then `open` or `ctrl` + `O`, search for `C:\Program Files (x86)\Steam\SteamApps\common\mordheim\mordheim_Data\Managed\Assembly-CSharp.dll` and open it.

You should now see on the left panel `Assembly-CSharp` along with a bunch of other which are mostly dependencies of Assembly-CSharp ( and dnSpy assemblies ).

In `Assembly-CSharp` you should see a list of `namespace` written in `yellow` and `{} -` which contains most of the game code.

We will take the Mirage Mod feature [MiragePopup](/MirageMod/MiragePopup.md) as a base for this tutorial.

Open the class `MainMenuStartView` in `{} -` or use `ctrl` + `shift` + `K` to search inside the assembly. 

To edit a method, use `right click` on the code then `Edit Method` or `ctrl` + `shift` + `E`.

Edit the method `Awake` and add the lines, between the `// ADD LINES - START` and `// ADD LINES - END` comments
```csharp
public override void Awake()
{
	base.Awake();
	// ADD LINES - START
	base.StateMachine.ConfirmPopup.Show("menu_warning","My first mod", 
	new Action<bool> (this.OnPopup), false, false);
	// ADD LINES - END
	this.btnContinue.onAction.AddListener(new UnityAction(this.OnContinueCampaign));
	this.btnLoadGame.onAction.AddListener(delegate()
```

Click on `Compile` at the bottom to save your modifications

Open `ConfirmationPopupView` or `ctrl` + `click` on `Show` from `base.StateMachine.ConfirmPopup.Show` in the code above

Then edit the method `Show` in the same way, `ctrl` + `F` inside the file to find a method
```csharp
public virtual void Show(string titleId, string textId, Action<bool> callback, bool hideButtons = false, bool hideCancel = false)
{
	if (!string.IsNullOrEmpty(titleId))
	{
		this.title.text = PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleId);
	}
	if (!string.IsNullOrEmpty(textId))
	{
		this.text.text = PandoraSingleton<LocalizationManager>.Instance.GetStringById(textId);
	}
	// ADD LINES - START
	if (textId == "My first mod")
	{
	this.text.text = "Bravo! You created your first mod!\nIf you click on Confirm it will close the game.";
	}
	// ADD LINES - END
	this.Show(callback, hideButtons, hideCancel);
}
```

Hit `Compile`

Click on `File` then `Save All` or `ctrl` + `shift` + `S`, then hit `OK`

Start your game and you should see a popup on the main screen :wink:



