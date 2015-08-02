/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 2.0.0
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */

namespace RakNet {

using System;
using System.Runtime.InteropServices;

public class TM_Team : IDisposable {
  private HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal TM_Team(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(TM_Team obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~TM_Team() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          RakNetPINVOKE.delete_TM_Team(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
    }
  }

  public static TM_Team GetInstance() {
    IntPtr cPtr = RakNetPINVOKE.TM_Team_GetInstance();
    TM_Team ret = (cPtr == IntPtr.Zero) ? null : new TM_Team(cPtr, false);
    return ret;
  }

  public static void DestroyInstance(TM_Team i) {
    RakNetPINVOKE.TM_Team_DestroyInstance(TM_Team.getCPtr(i));
  }

  public TM_Team() : this(RakNetPINVOKE.new_TM_Team(), true) {
  }

  public bool SetMemberLimit(ushort _teamMemberLimit, byte noTeamSubcategory) {
    bool ret = RakNetPINVOKE.TM_Team_SetMemberLimit(swigCPtr, _teamMemberLimit, noTeamSubcategory);
    return ret;
  }

  public ushort GetMemberLimit() {
    ushort ret = RakNetPINVOKE.TM_Team_GetMemberLimit(swigCPtr);
    return ret;
  }

  public ushort GetMemberLimitSetting() {
    ushort ret = RakNetPINVOKE.TM_Team_GetMemberLimitSetting(swigCPtr);
    return ret;
  }

  public bool SetJoinPermissions(byte _joinPermissions) {
    bool ret = RakNetPINVOKE.TM_Team_SetJoinPermissions(swigCPtr, _joinPermissions);
    return ret;
  }

  public byte GetJoinPermissions() {
    byte ret = RakNetPINVOKE.TM_Team_GetJoinPermissions(swigCPtr);
    return ret;
  }

  public void LeaveTeam(TM_TeamMember teamMember, byte noTeamSubcategory) {
    RakNetPINVOKE.TM_Team_LeaveTeam(swigCPtr, TM_TeamMember.getCPtr(teamMember), noTeamSubcategory);
  }

  public bool GetBalancingApplies() {
    bool ret = RakNetPINVOKE.TM_Team_GetBalancingApplies(swigCPtr);
    return ret;
  }

  public void GetTeamMembers(SWIGTYPE_p_DataStructures__ListT_RakNet__TM_TeamMember_p_t _teamMembers) {
    RakNetPINVOKE.TM_Team_GetTeamMembers(swigCPtr, SWIGTYPE_p_DataStructures__ListT_RakNet__TM_TeamMember_p_t.getCPtr(_teamMembers));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public uint GetTeamMembersCount() {
    uint ret = RakNetPINVOKE.TM_Team_GetTeamMembersCount(swigCPtr);
    return ret;
  }

  public TM_TeamMember GetTeamMemberByIndex(uint index) {
    IntPtr cPtr = RakNetPINVOKE.TM_Team_GetTeamMemberByIndex(swigCPtr, index);
    TM_TeamMember ret = (cPtr == IntPtr.Zero) ? null : new TM_TeamMember(cPtr, false);
    return ret;
  }

  public ulong GetNetworkID() {
    ulong ret = RakNetPINVOKE.TM_Team_GetNetworkID(swigCPtr);
    return ret;
  }

  public TM_World GetTM_World() {
    IntPtr cPtr = RakNetPINVOKE.TM_Team_GetTM_World(swigCPtr);
    TM_World ret = (cPtr == IntPtr.Zero) ? null : new TM_World(cPtr, false);
    return ret;
  }

  public void SerializeConstruction(BitStream constructionBitstream) {
    RakNetPINVOKE.TM_Team_SerializeConstruction(swigCPtr, BitStream.getCPtr(constructionBitstream));
  }

  public bool DeserializeConstruction(TeamManager teamManager, BitStream constructionBitstream) {
    bool ret = RakNetPINVOKE.TM_Team_DeserializeConstruction(swigCPtr, TeamManager.getCPtr(teamManager), BitStream.getCPtr(constructionBitstream));
    return ret;
  }

  public void SetOwner(SWIGTYPE_p_void o) {
    RakNetPINVOKE.TM_Team_SetOwner(swigCPtr, SWIGTYPE_p_void.getCPtr(o));
  }

  public SWIGTYPE_p_void GetOwner() {
    IntPtr cPtr = RakNetPINVOKE.TM_Team_GetOwner(swigCPtr);
    SWIGTYPE_p_void ret = (cPtr == IntPtr.Zero) ? null : new SWIGTYPE_p_void(cPtr, false);
    return ret;
  }

  public uint GetWorldIndex() {
    uint ret = RakNetPINVOKE.TM_Team_GetWorldIndex(swigCPtr);
    return ret;
  }

  public static uint ToUint32(ulong g) {
    uint ret = RakNetPINVOKE.TM_Team_ToUint32(g);
    return ret;
  }

}

}
