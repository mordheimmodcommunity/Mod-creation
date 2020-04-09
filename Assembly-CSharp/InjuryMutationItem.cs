using UnityEngine;
using UnityEngine.UI;

public class InjuryMutationItem : MonoBehaviour
{
	public Text title;

	public Text desc;

	public Text rating;

	public void Set(Injury inj)
	{
		Set(inj.LocName, inj.LocDesc, inj.Data.Rating.ToConstantString());
	}

	public void Set(Mutation mut)
	{
		Set(mut.LocName, mut.LocDesc, mut.Data.Rating.ToConstantString());
	}

	public void Set(string title, string desc, string rating = "0")
	{
		this.title.set_text(title);
		this.desc.set_text(desc);
		this.rating.set_text(rating);
	}
}
