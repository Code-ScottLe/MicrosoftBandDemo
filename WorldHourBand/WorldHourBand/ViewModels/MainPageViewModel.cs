using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Band;
using System.ComponentModel;

namespace WorldHourBand.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Fields
        /// <summary>
        /// representing the info of the current Band that we are connected to.
        /// </summary>
        private IBandInfo currentBandInfo;

        /// <summary>
        /// representing the band client that we are working with.
        /// </summary>
        private IBandClient currentBandClient;

        #endregion

        #region Properties
        /// <summary>
        /// Implement the INotifyPropertyChanged interface for databinding with the main page
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        #region Constructors

        /// <summary>
        /// Default constructor for the MainPageViewModel.
        /// </summary>
        public MainPageViewModel()
        {

        }
        #endregion

        #region methods

        /// <summary>
        /// Initialize the connection to the band. Default will be the first one in the paired Band 
        /// </summary>
        /// <returns> True indicate success , False indicate failure</returns>
        public async Task<bool> InitializeBandConnection()
        {

            //check if we already have a connection to a band or not.
            if (currentBandInfo == null)
            {
                //Get a list of paired band
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();

                
                if(pairedBands.Count() < 1)
                {
                    //something is wrong.
                    return false;
                }

                //Get the band info 
                currentBandInfo = pairedBands[0];
            }

            
            try
            {
                //Try to establish connection to the band
                currentBandClient = await BandClientManager.Instance.ConnectAsync(currentBandInfo);

                return true;
            }

            catch(BandException ex)
            {
                //something went wrong.
                return false;
            }   

        } 

        /// <summary>
        /// Invoke the PropertyChanged event from the INotifyPropertyChanged Interface with a custom message.
        /// </summary>
        /// <param name="message"> Message about the event</param>
        public void OnPropertyChanged(string message)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(message));
            }
        }

        #endregion


    }
}
