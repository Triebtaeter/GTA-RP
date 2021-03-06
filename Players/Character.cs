﻿using System;
using GTA_RP.Factions;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Shared.Math;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using System.Collections.Generic;
using GTA_RP.Items;

namespace GTA_RP
{
    public enum StatusEffect
    {
        HANDCUFFED = 0
    }

    /// <summary>
    /// Class for characters in game
    /// Every character that is in the game world is represented by this class
    /// </summary>
    public class Character
    {
        public int ID { get; private set; }
        public int job { get; private set; }
        public Player owner { get; private set; }
        public String firstName { get; private set; }
        public String lastName { get; private set; }
        public String model { get; private set; }
        public Phone phone { get; private set; }
        public int gender { get; private set; }
        public int spawnHouseId { get; private set; }

        public Inventory inventory { get; private set; }

        public String fullName
        {
            get { return firstName + " " + lastName; }
        }

        public Client client
        {
            get { return this.owner.client; }
        }

        public Vector3 position
        {
            get { return owner.client.position; }
            set { owner.client.position = value; }
        }

        private int moneyPrivate;
        public int money
        {
            get { return moneyPrivate; }
            set
            {
                moneyPrivate = value;
                API.shared.triggerClientEvent(owner.client, "EVENT_UPDATE_MONEY", moneyPrivate.ToString());
            }
        }

        public Faction faction
        {
            get { return FactionManager.Instance().GetFactionWithId(this.factionID); }
        }

        public Boolean isInVehicle
        {
            get { return API.shared.isPlayerInAnyVehicle(this.owner.client); }
        }

        public Boolean isDriver
        {
            get
            {
                if (API.shared.getPlayerVehicleSeat(this.owner.client) == -1)
                    return true;
                return false;
            }
        }

        public int vehicleClass
        {
            get
            {
                // Confirm working
                return API.shared.getVehicleClass((VehicleHash)API.shared.getEntityModel(API.shared.getPlayerVehicle(this.owner.client)));
            }

        }

        public FactionI factionID { get; private set; }
        public Boolean onDuty { get; set; }
        private List<StatusEffect> statusEffects = new List<StatusEffect>();


        public Character(Player owner, int ID, String firstName, String lastName, FactionI factionID, string model, int money, int job, String phoneNumber, int spawnHouseId)
        {
            this.owner = owner;
            this.model = model;
            this.onDuty = false;
            this.factionID = factionID;
            this.ID = ID;
            this.firstName = firstName;
            this.lastName = lastName;
            this.money = money;
            this.job = job;
            this.gender = PlayerManager.Instance().GetGenderForModel(model);
            this.phone = new Phone(this, phoneNumber);
            this.spawnHouseId = spawnHouseId;
            this.inventory = new Inventory(this);
        }

        /// <summary>
        /// Destroys attached objects
        /// </summary>
        public void CleanUp()
        {
        }

        /// <summary>
        /// Checks if two characters are the same
        /// </summary>
        /// <param name="obj">Character</param>
        /// <returns>True if character ids are same, otherwise false</returns>
        public override bool Equals(object obj)
        {
            var second = obj as Character;

            if (second == null)
                return false;
            
            return this.ID.Equals(second.ID);
        }

        /// <summary>
        /// Gets hashcode of character id
        /// </summary>
        /// <returns>Hashcode of character id</returns>
        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        /// <summary>
        /// Checks if character has a status effect
        /// </summary>
        /// <param name="e">Status effect to check</param>
        /// <returns>True if player has the status effect, otherwise false</returns>
        public Boolean HasStatusEffect(StatusEffect e)
        {
            return statusEffects.Contains(e);
        }

        public void UseItemInInventory(int itemId)
        {
            this.inventory.UseItemWithId(itemId);
        }

        /// <summary>
        /// Adds status effect to player
        /// </summary>
        /// <param name="e">Status effect to add</param>
        public void AddStatusEffect(StatusEffect e)
        {
            statusEffects.Add(e);
        }

        /// <summary>
        /// Removes status effect from player
        /// </summary>
        /// <param name="e">Status effect to remove</param>
        public void RemoveStatusEffect(StatusEffect e)
        {
            statusEffects.Remove(e);
        }

        /// <summary>
        /// Sets character money
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="updateDatabase">Whether the new amount will be updated to the database</param>
        public void SetMoney(int amount, bool updateDatabase = true)
        {
            this.money = amount;
            if (updateDatabase) PlayerManager.Instance().UpdateCharacterMoneyToDatabase(this, this.money);
        }

        /// <summary>
        /// Sets the job for character
        /// </summary>
        /// <param name="jobId">ID of the job</param>
        public void SetJob(int jobId)
        {
            this.job = jobId;
        }

        /// <summary>
        /// Updates "job text" for the clientside of this character
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="colorR">R color</param>
        /// <param name="colorG">G color</param>
        /// <param name="colorB">B color</param>
        public void UpdateFactionRankText(string text, int colorR, int colorG, int colorB)
        {
            this.TriggerEvent("EVENT_UPDATE_JOB", text, colorR, colorG, colorB);
        }

        /// <summary>
        /// Adds item to the character's inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        public void AddItemToInventory(Item item, bool updateDB, bool updateUI = true)
        {
            this.inventory.AddItem(item, updateDB);
            if (updateUI) API.shared.triggerClientEvent(this.client, "EVENT_ADD_ITEM_TO_INVENTORY", item.id, item.name, item.count, item.description);

        }

        public bool HasItemWithid(int id, int count)
        {
            return inventory.DoesContainItemWithIdAndCount(id, count);
        }

        public void TriggerEvent(string name, params object[] args)
        {
            this.client.triggerEvent(name, args);
        }

        
        public void RemoveItemFromInventory(int itemId, int count, bool updateDB, bool updateUI = true)
        {
            this.inventory.RemoveItemWithId(itemId, count);
            if (updateUI) this.client.triggerEvent("EVENT_REMOVE_ITEM_FROM_INVENTORY", itemId, count); // check
        }

        public int GetAmountOfItems(int itemId)
        {
            return inventory.GetItemCount(itemId);
        }

        public List<Item> GetAllItemsFromInventory()
        {
            return this.inventory.GetAllItems();
        }

        public void SetModel(string modelName)
        {
            this.client.setSkin(API.shared.pedNameToModel(modelName));
        }

        /// <summary>
        /// Plays animation for character
        /// </summary>
        /// <param name="flag">Flags for animation</param>
        /// <param name="animDict">Animation dictionary name</param>
        /// <param name="animName">Animation name</param>
        public void PlayAnimation(int flag, string animDict, string animName)
        {
            if (this.owner != null)
                API.shared.playPlayerAnimation(this.owner.client, flag, animDict, animName);
        }

        public void StopAnimation()
        {
            API.shared.stopPlayerAnimation(this.client);
        }

        public void PlayFrontendSound(string name, string set)
        {
            API.shared.playSoundFrontEnd(this.client, name, set);
        }

        public void SendNotification(string message)
        {
            API.shared.sendNotificationToPlayer(this.client, message);
        }

        public bool IsAvailable()
        {
            if (!client.isAiming && !client.inFreefall && !client.isInCover && !client.isParachuting && !client.isReloading && !client.isShooting && !client.dead)
                return true;
            return false;
        }

        /// <summary>
        /// Attaches object to character
        /// </summary>
        /// <param name="entity">Object to attach</param>
        /// <param name="bone">Bone to attach object to</param>
        /// <param name="posOffset">Offset of attach position</param>
        /// <param name="rotOffset">Rotation offset of attach position</param>
        public void AttachObject(NetHandle entity, string bone, Vector3 posOffset, Vector3 rotOffset)
        {
            if (this.owner != null)
                API.shared.attachEntityToEntity(entity, owner.client.handle, bone, posOffset, rotOffset);
        }

        public bool IsUsingPhone()
        {
            return phone.IsUsingPhone();
        }

        /// <summary>
        /// Detaches object from the character
        /// </summary>
        /// <param name="entity">Object to detach</param>
        public void DetachObject(NetHandle entity)
        {
            API.shared.detachEntity(entity);
        }
    }
}
