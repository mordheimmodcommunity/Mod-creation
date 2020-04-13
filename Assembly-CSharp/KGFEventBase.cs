using System;

[Serializable]
public abstract class KGFEventBase : KGFObject, KGFIValidator
{
    public abstract void Trigger();

    public virtual KGFMessageList Validate()
    {
        return new KGFMessageList();
    }
}
