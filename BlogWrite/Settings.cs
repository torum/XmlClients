namespace BlogWrite.Properties {

    //  SettingChanging
    //  PropertyChanged
    //  SettingsLoaded
    //  SettingsSaving
    public sealed partial class Settings {
        
        public Settings() {
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // SettingChangingEvent
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // SettingsSaving 
        }
    }
}
