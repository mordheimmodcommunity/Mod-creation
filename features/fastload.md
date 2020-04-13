# Feature: fastload

### CopyrightManager.cs


```csharp
 public void update
	{
		...code..
-   this.fadeToMainMenu(); // remove this line
    ...code...
	}
```

```csharp
 public void start
	{
		...code..
+   this.fadeToMainMenu(); // add this line
	}
```

`Fastload as it skips the Intros. Comment: It would be nice to use a MODPICTURE in the loading screen!`