/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;
using libsecondlife;

namespace OpenSim.Framework.Data
{
    public class AvatarPickerAvatar
    {
        public LLUUID AvatarID;
        public string firstName;
        public string lastName;

        public AvatarPickerAvatar()
        {
        }
    }

    public enum DataResponse
    {
        RESPONSE_OK,
        RESPONSE_AUTHREQUIRED,
        RESPONSE_INVALIDCREDENTIALS,
        RESPONSE_ERROR
    }

    /// <summary>
    /// A standard grid interface
    /// </summary>
    public interface IGridData
    {
        /// <summary>
        /// Returns a sim profile from a regionHandle
        /// </summary>
        /// <param name="regionHandle">A 64bit Region Handle</param>
        /// <returns>A simprofile</returns>
        RegionProfileData GetProfileByHandle(ulong regionHandle);

        /// <summary>
        /// Returns a sim profile from a UUID
        /// </summary>
        /// <param name="UUID">A 128bit UUID</param>
        /// <returns>A sim profile</returns>
        RegionProfileData GetProfileByLLUUID(LLUUID UUID);

        /// <summary>
        /// Returns all profiles within the specified range
        /// </summary>
        /// <param name="Xmin">Minimum sim coordinate (X)</param>
        /// <param name="Ymin">Minimum sim coordinate (Y)</param>
        /// <param name="Xmax">Maximum sim coordinate (X)</param>
        /// <param name="Ymin">Maximum sim coordinate (Y)</param>
        /// <returns>An array containing all the sim profiles in the specified range</returns>
        RegionProfileData[] GetProfilesInRange(uint Xmin, uint Ymin, uint Xmax, uint Ymax);

        /// <summary>
        /// Authenticates a sim by use of its recv key. 
        /// WARNING: Insecure
        /// </summary>
        /// <param name="UUID">The UUID sent by the sim</param>
        /// <param name="regionHandle">The regionhandle sent by the sim</param>
        /// <param name="simrecvkey">The receiving key sent by the sim</param>
        /// <returns>Whether the sim has been authenticated</returns>
        bool AuthenticateSim(LLUUID UUID, ulong regionHandle, string simrecvkey);

        /// <summary>
        /// Initialises the interface
        /// </summary>
        void Initialise();

        /// <summary>
        /// Closes the interface
        /// </summary>
        void Close();

        /// <summary>
        /// The plugin being loaded
        /// </summary>
        /// <returns>A string containing the plugin name</returns>
        string getName();

        /// <summary>
        /// The plugins version
        /// </summary>
        /// <returns>A string containing the plugin version</returns>
        string getVersion();

        /// <summary>
        /// Adds a new profile to the database
        /// </summary>
        /// <param name="profile">The profile to add</param>
        /// <returns>RESPONSE_OK if successful, error if not.</returns>
        DataResponse AddProfile(RegionProfileData profile);

        ReservationData GetReservationAtPoint(uint x, uint y);
    }
}
