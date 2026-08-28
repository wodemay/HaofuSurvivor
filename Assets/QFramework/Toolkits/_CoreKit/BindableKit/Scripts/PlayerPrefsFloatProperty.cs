namespace QFramework
{
    public class RuntimeFloatProperty : BindableProperty<float>
    {
        public RuntimeFloatProperty(string key, float defaultValue = 0.0f)
        {
            mValue = RuntimeSettingsStorage.GetFloat(key, defaultValue);
            Register(value => RuntimeSettingsStorage.SetFloat(key, value));
        }
    }
}
