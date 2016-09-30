namespace requestPeso
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione componenti

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.requestPesoProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.requestPeso = new System.ServiceProcess.ServiceInstaller();
            // 
            // requestPesoProcessInstaller
            // 
            this.requestPesoProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.requestPesoProcessInstaller.Password = null;
            this.requestPesoProcessInstaller.Username = null;
            // 
            // requestPeso
            // 
            this.requestPeso.Description = "Richiede il peso alla bilancia collegata sulla seriale";
            this.requestPeso.ServiceName = "requestPeso";
            this.requestPeso.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.requestPesoProcessInstaller,
            this.requestPeso});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller requestPesoProcessInstaller;
        private System.ServiceProcess.ServiceInstaller requestPeso;
    }
}