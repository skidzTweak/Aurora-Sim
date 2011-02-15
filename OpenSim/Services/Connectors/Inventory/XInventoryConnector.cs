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
 *     * Neither the name of the OpenSimulator Project nor the
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

using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Framework.Console;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Services.Interfaces;
using Aurora.Simulation.Base;
using OpenMetaverse;

namespace OpenSim.Services.Connectors
{
    public class XInventoryServicesConnector : IInventoryService, IService
    {
        private static readonly ILog m_log =
                LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private List<string> m_ServerURIs = new List<string>();

        public virtual bool CreateUserInventory(UUID principalID)
        {
            Dictionary<string,object> ret = MakeRequest("CREATEUSERINVENTORY",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() }
                    });

            if (ret == null)
                return false;
            if (ret.Count == 0)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual List<InventoryFolderBase> GetInventorySkeleton(UUID principalID)
        {
            Dictionary<string,object> ret = MakeRequest("GETINVENTORYSKELETON",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() }
                    });

            if (ret == null)
                return null;
            if (ret.Count == 0)
                return null;

            List<InventoryFolderBase> folders = new List<InventoryFolderBase>();

            try
            {
                foreach (Object o in ret.Values)
                    folders.Add(BuildFolder((Dictionary<string, object>)o));
            }
            catch (Exception e)
            {
                m_log.DebugFormat("[XINVENTORY CONNECTOR STUB]: Exception unwrapping folder list: {0}", e.Message);
            }

            return folders;
        }

        public virtual InventoryFolderBase GetRootFolder(UUID principalID)
        {
            Dictionary<string,object> ret = MakeRequest("GETROOTFOLDER",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() }
                    });

            if (ret == null)
                return null;
            if (ret.Count == 0)
                return null;

            return BuildFolder((Dictionary<string, object>)ret["folder"]);
        }

        public virtual InventoryFolderBase GetFolderForType(UUID principalID, AssetType type)
        {
            Dictionary<string,object> ret = MakeRequest("GETFOLDERFORTYPE",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() },
                        { "TYPE", ((int)type).ToString() }
                    });

            if (ret == null)
                return null;
            if (ret.Count == 0)
                return null;

            return BuildFolder((Dictionary<string, object>)ret["folder"]);
        }

        public virtual InventoryCollection GetFolderContent(UUID principalID, UUID folderID)
        {
            InventoryCollection inventory = new InventoryCollection();
            
            try
            {
                Dictionary<string,object> ret = MakeRequest("GETFOLDERCONTENT",
                        new Dictionary<string,object> {
                            { "PRINCIPAL", principalID.ToString() },
                            { "FOLDER", folderID.ToString() }
                        });

                if (ret == null)
                    return null;
                if (ret.Count == 0)
                    return null;

                
                inventory.Folders = new List<InventoryFolderBase>();
                inventory.Items = new List<InventoryItemBase>();
                inventory.UserID = principalID;
                
                Dictionary<string,object> folders =
                        (Dictionary<string,object>)ret["FOLDERS"];
                Dictionary<string,object> items =
                        (Dictionary<string,object>)ret["ITEMS"];

                foreach (Object o in folders.Values) // getting the values directly, we don't care about the keys folder_i
                    inventory.Folders.Add(BuildFolder((Dictionary<string, object>)o));
                foreach (Object o in items.Values) // getting the values directly, we don't care about the keys item_i
                    inventory.Items.Add(BuildItem((Dictionary<string, object>)o));
            }
            catch (Exception e)
            {
                m_log.DebugFormat("[XINVENTORY CONNECTOR STUB]: Exception in GetFolderContent: {0}", e.Message);
            }

            return inventory;
        }

        public virtual List<InventoryItemBase> GetFolderItems(UUID principalID, UUID folderID)
        {
            Dictionary<string,object> ret = MakeRequest("GETFOLDERITEMS",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() },
                        { "FOLDER", folderID.ToString() }
                    });

            if (ret == null)
                return null;
            if (ret.Count == 0)
                return null;

            Dictionary<string, object> items = (Dictionary<string, object>)ret["ITEMS"];
            List<InventoryItemBase> fitems = new List<InventoryItemBase>();
            foreach (Object o in items.Values) // getting the values directly, we don't care about the keys item_i
                fitems.Add(BuildItem((Dictionary<string, object>)o));

            return fitems;
        }

        public virtual bool AddFolder(InventoryFolderBase folder)
        {
            if (folder == null)
                return false;

            Dictionary<string, object> ret = MakeRequest("ADDFOLDER",
                    new Dictionary<string,object> {
                        { "ParentID", folder.ParentID.ToString() },
                        { "Type", folder.Type.ToString() },
                        { "Version", folder.Version.ToString() },
                        { "Name", folder.Name.ToString() },
                        { "Owner", folder.Owner.ToString() },
                        { "ID", folder.ID.ToString() }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool UpdateFolder(InventoryFolderBase folder)
        {
            if (folder == null)
                return false;

            Dictionary<string, object> ret = MakeRequest("UPDATEFOLDER",
                    new Dictionary<string,object> {
                        { "ParentID", folder.ParentID.ToString() },
                        { "Type", folder.Type.ToString() },
                        { "Version", folder.Version.ToString() },
                        { "Name", folder.Name.ToString() },
                        { "Owner", folder.Owner.ToString() },
                        { "ID", folder.ID.ToString() }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool MoveFolder(InventoryFolderBase folder)
        {
            if (folder == null)
                return false;

            Dictionary<string, object> ret = MakeRequest("MOVEFOLDER",
                    new Dictionary<string,object> {
                        { "ParentID", folder.ParentID.ToString() },
                        { "ID", folder.ID.ToString() },
                        { "PRINCIPAL", folder.Owner.ToString() }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool DeleteFolders(UUID principalID, List<UUID> folderIDs)
        {
            if (folderIDs == null)
                return false;
            if (folderIDs.Count == 0)
                return false;

            List<string> slist = new List<string>();

            foreach (UUID f in folderIDs)
                slist.Add(f.ToString());

            Dictionary<string,object> ret = MakeRequest("DELETEFOLDERS",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() },
                        { "FOLDERS", slist }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool PurgeFolder(InventoryFolderBase folder)
        {
            if (folder == null)
                return false;

            Dictionary<string, object> ret = MakeRequest("PURGEFOLDER",
                    new Dictionary<string,object> {
                        { "ID", folder.ID.ToString() }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool AddItem(InventoryItemBase item)
        {
            if (item == null)
                return false;

            Dictionary<string, object> ret = MakeRequest("ADDITEM",
                    new Dictionary<string,object> {
                        { "AssetID", item.AssetID.ToString() },
                        { "AssetType", item.AssetType.ToString() },
                        { "Name", item.Name.ToString() },
                        { "Owner", item.Owner.ToString() },
                        { "ID", item.ID.ToString() },
                        { "InvType", item.InvType.ToString() },
                        { "Folder", item.Folder.ToString() },
                        { "CreatorId", item.CreatorId.ToString() },
                        { "Description", item.Description.ToString() },
                        { "NextPermissions", item.NextPermissions.ToString() },
                        { "CurrentPermissions", item.CurrentPermissions.ToString() },
                        { "BasePermissions", item.BasePermissions.ToString() },
                        { "EveryOnePermissions", item.EveryOnePermissions.ToString() },
                        { "GroupPermissions", item.GroupPermissions.ToString() },
                        { "GroupID", item.GroupID.ToString() },
                        { "GroupOwned", item.GroupOwned.ToString() },
                        { "SalePrice", item.SalePrice.ToString() },
                        { "SaleType", item.SaleType.ToString() },
                        { "Flags", item.Flags.ToString() },
                        { "CreationDate", item.CreationDate.ToString() }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool UpdateItem(InventoryItemBase item)
        {
            if (item == null)
                return false;

            Dictionary<string, object> ret = MakeRequest("UPDATEITEM",
                    new Dictionary<string,object> {
                        { "AssetID", item.AssetID.ToString() },
                        { "AssetType", item.AssetType.ToString() },
                        { "Name", item.Name.ToString() },
                        { "Owner", item.Owner.ToString() },
                        { "ID", item.ID.ToString() },
                        { "InvType", item.InvType.ToString() },
                        { "Folder", item.Folder.ToString() },
                        { "CreatorId", item.CreatorId.ToString() },
                        { "Description", item.Description.ToString() },
                        { "NextPermissions", item.NextPermissions.ToString() },
                        { "CurrentPermissions", item.CurrentPermissions.ToString() },
                        { "BasePermissions", item.BasePermissions.ToString() },
                        { "EveryOnePermissions", item.EveryOnePermissions.ToString() },
                        { "GroupPermissions", item.GroupPermissions.ToString() },
                        { "GroupID", item.GroupID.ToString() },
                        { "GroupOwned", item.GroupOwned.ToString() },
                        { "SalePrice", item.SalePrice.ToString() },
                        { "SaleType", item.SaleType.ToString() },
                        { "Flags", item.Flags.ToString() },
                        { "CreationDate", item.CreationDate.ToString() }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool MoveItems(UUID principalID, List<InventoryItemBase> items)
        {
            if (items == null)
                return false;

            List<string> idlist = new List<string>();
            List<string> destlist = new List<string>();

            foreach (InventoryItemBase item in items)
            {
                idlist.Add(item.ID.ToString());
                destlist.Add(item.Folder.ToString());
            }

            Dictionary<string,object> ret = MakeRequest("MOVEITEMS",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() },
                        { "IDLIST", idlist },
                        { "DESTLIST", destlist }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual bool DeleteItems(UUID principalID, List<UUID> itemIDs)
        {
            if (itemIDs == null)
                return false;
            if (itemIDs.Count == 0)
                return true;

            List<string> slist = new List<string>();

            foreach (UUID f in itemIDs)
                slist.Add(f.ToString());

            Dictionary<string,object> ret = MakeRequest("DELETEITEMS",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() },
                        { "ITEMS", slist }
                    });

            if (ret == null)
                return false;

            return bool.Parse(ret["RESULT"].ToString());
        }

        public virtual InventoryItemBase GetItem(InventoryItemBase item)
        {
            if (item == null)
                return null;

            try
            {
                Dictionary<string, object> ret = MakeRequest("GETITEM",
                        new Dictionary<string, object> {
                        { "ID", item.ID.ToString() }
                    });

                if (ret == null)
                    return null;
                if (ret.Count == 0)
                    return null;

                return BuildItem((Dictionary<string, object>)ret["item"]);
            }
            catch (Exception e)
            {
                m_log.DebugFormat("[XINVENTORY CONNECTOR STUB]: Exception in GetItem: {0}", e.Message);
            }

            return null;
        }

        public virtual InventoryFolderBase GetFolder(InventoryFolderBase folder)
        {
            if (folder == null)
                return null;

            try
            {
                Dictionary<string, object> ret = MakeRequest("GETFOLDER",
                        new Dictionary<string, object> {
                        { "ID", folder.ID.ToString() }
                    });

                if (ret == null)
                    return null;
                if (ret.Count == 0)
                    return null;

                return BuildFolder((Dictionary<string, object>)ret["folder"]);
            }
            catch (Exception e)
            {
                m_log.DebugFormat("[XINVENTORY CONNECTOR STUB]: Exception in GetFolder: {0}", e.Message);
            }

            return null;
        }

        public virtual List<InventoryItemBase> GetActiveGestures(UUID principalID)
        {
            Dictionary<string,object> ret = MakeRequest("GETACTIVEGESTURES",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() }
                    });

            if (ret == null)
                return null;

            List<InventoryItemBase> items = new List<InventoryItemBase>();

            foreach (Object o in ret.Values) // getting the values directly, we don't care about the keys item_i
                items.Add(BuildItem((Dictionary<string, object>)o));

            return items;
        }

        public virtual int GetAssetPermissions(UUID principalID, UUID assetID)
        {
            Dictionary<string,object> ret = MakeRequest("GETASSETPERMISSIONS",
                    new Dictionary<string,object> {
                        { "PRINCIPAL", principalID.ToString() },
                        { "ASSET", assetID.ToString() }
                    });

            if (ret == null)
                return 0;

            return int.Parse(ret["RESULT"].ToString());
        }


        public virtual bool HasInventoryForUser(UUID principalID)
        {
            return false;
        }

        // Helpers
        //
        private Dictionary<string,object> MakeRequest(string method,
                Dictionary<string,object> sendData)
        {
            sendData["METHOD"] = method;

            foreach (string m_ServerURI in m_ServerURIs)
            {
                string reply = SynchronousRestFormsRequester.MakeRequest("POST",
                    m_ServerURI + "/xinventory",
                    WebUtils.BuildQueryString(sendData));

                Dictionary<string, object> replyData = WebUtils.ParseXmlResponse(
                        reply);

                return replyData;
            }
            return null;
        }

        private InventoryFolderBase BuildFolder(Dictionary<string,object> data)
        {
            InventoryFolderBase folder = new InventoryFolderBase();

            try
            {
                folder.ParentID = new UUID(data["ParentID"].ToString());
                folder.Type = short.Parse(data["Type"].ToString());
                folder.Version = ushort.Parse(data["Version"].ToString());
                folder.Name = data["Name"].ToString();
                folder.Owner = new UUID(data["Owner"].ToString());
                folder.ID = new UUID(data["ID"].ToString());
            }
            catch (Exception e)
            {
                m_log.DebugFormat("[XINVENTORY CONNECTOR STUB]: Exception building folder: {0}", e.Message);
            }

            return folder;
        }

        private InventoryItemBase BuildItem(Dictionary<string,object> data)
        {
            InventoryItemBase item = new InventoryItemBase();

            try
            {
                item.AssetID = new UUID(data["AssetID"].ToString());
                item.AssetType = int.Parse(data["AssetType"].ToString());
                item.Name = data["Name"].ToString();
                item.Owner = new UUID(data["Owner"].ToString());
                item.ID = new UUID(data["ID"].ToString());
                item.InvType = int.Parse(data["InvType"].ToString());
                item.Folder = new UUID(data["Folder"].ToString());
                item.CreatorId = data["CreatorId"].ToString();
                item.Description = data["Description"].ToString();
                item.NextPermissions = uint.Parse(data["NextPermissions"].ToString());
                item.CurrentPermissions = uint.Parse(data["CurrentPermissions"].ToString());
                item.BasePermissions = uint.Parse(data["BasePermissions"].ToString());
                item.EveryOnePermissions = uint.Parse(data["EveryOnePermissions"].ToString());
                item.GroupPermissions = uint.Parse(data["GroupPermissions"].ToString());
                item.GroupID = new UUID(data["GroupID"].ToString());
                item.GroupOwned = bool.Parse(data["GroupOwned"].ToString());
                item.SalePrice = int.Parse(data["SalePrice"].ToString());
                item.SaleType = byte.Parse(data["SaleType"].ToString());
                item.Flags = uint.Parse(data["Flags"].ToString());
                item.CreationDate = int.Parse(data["CreationDate"].ToString());
            }
            catch (Exception e)
            {
                m_log.DebugFormat("[XINVENTORY CONNECTOR STUB]: Exception building item: {0}", e.Message);
            }

            return item;
        }

        #region IService Members

        public virtual string Name
        {
            get { return GetType().Name; }
        }

        public virtual void Initialize(IConfigSource config, IRegistryCore registry)
        {
            IConfig handlerConfig = config.Configs["Handlers"];
            if (handlerConfig.GetString("InventoryHandler", "") != Name)
                return;

            m_ServerURIs = registry.RequestModuleInterface<IConfigurationService>().FindValueOf("InventoryServerURI");
            registry.RegisterModuleInterface<IInventoryService>(this);
        }

        public virtual void Start(IConfigSource config, IRegistryCore registry)
        {
        }

        public void AddNewRegistry(IConfigSource config, IRegistryCore registry)
        {
            IConfig handlerConfig = config.Configs["Handlers"];
            if (handlerConfig.GetString("InventoryHandler", "") != Name)
                return;

            registry.RegisterModuleInterface<IInventoryService>(this);
        }

        #endregion
    }
}
