using System;
using System.Collections.Generic;
using System.Text;

namespace DenonLib
{
    public interface IDenonDevice
    {
        #region Power ========================================================================================================
        void PowerOn();
        void PowerStandby();
        #endregion

        #region Master Volume ========================================================================================================
        /// <summary>
        /// Returns the master volume
        /// </summary>
        decimal GetMasterVolume();

        /// <summary>
        /// Set the master volume to a value between 0 and 98 inclusive
        /// </summary>
        /// <param name="volume"></param>
        void SetMasterVolume(decimal volume);

        /// <summary>
        /// Increment master volume by .5
        /// </summary>
        void MasterVolumeUp();

        /// <summary>
        /// Decrement master volume by .5
        /// </summary>
        void MasterVolumeDown();
        #endregion

        #region Channel Volume ========================================================================================================
        /// <summary>
        /// Increment channel volume by .5
        /// </summary>
        void ChannelVolumeUp(Channel channel);

        /// <summary>
        /// Increment channel volume by .5
        /// </summary>
        void ChannelVolumeDown(Channel channel);

        /// <summary>
        /// Set channel volume to a value between 38 and 62 inclusive
        /// </summary>
        void SetChannelVolume(Channel channel, decimal volume);

        /// <summary>
        /// Gets the channel volume status
        /// </summary>
        Dictionary<Channel, decimal> GetChannelStatus();

        /// <summary>
        /// Reset all channels to factory settings
        /// </summary>
        void ResetChannels();

        #endregion
    }
}
