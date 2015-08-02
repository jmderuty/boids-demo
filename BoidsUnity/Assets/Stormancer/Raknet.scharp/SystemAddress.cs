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
#pragma warning disable 0660

public class SystemAddress : IDisposable {
  private HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal SystemAddress(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new HandleRef(this, cPtr);
  }

  internal static HandleRef getCPtr(SystemAddress obj) {
    return (obj == null) ? new HandleRef(null, IntPtr.Zero) : obj.swigCPtr;
  }

  ~SystemAddress() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          RakNetPINVOKE.delete_SystemAddress(swigCPtr);
        }
        swigCPtr = new HandleRef(null, IntPtr.Zero);
      }
      GC.SuppressFinalize(this);
    }
  }


	public override int GetHashCode()
	{    
		// return (int)((this.port+this.binaryAddress)% int.MaxValue);
		return (int) ToInteger(this);
	}
	public static bool operator ==(SystemAddress a, SystemAddress b)
	{
 	   	// If both are null, or both are same instance, return true.
 		if (System.Object.ReferenceEquals(a, b))
 		{
 	       		return true;
 	   	}

  		// If one is null, but not both, return false.
   	 	if (((object)a == null) || ((object)b == null))
    		{
       		 	return false;
    		}

		    return a.Equals(b);//Equals should be overloaded as well
	}

	public static bool operator !=(SystemAddress a, SystemAddress b)
	{
   		 return a.OpNotEqual(b);
	}

	public static bool operator < (SystemAddress a, SystemAddress b)
	{
    		return a.OpLess(b);
	}

	public static bool operator >(SystemAddress a, SystemAddress b)
	{
		return a.OpGreater(b);
	}

	public static bool operator <=(SystemAddress a, SystemAddress b)
	{
		return (a.OpLess(b) || a==b);
	}

	public static bool operator >=(SystemAddress a, SystemAddress b)
	{
		return (a.OpGreater(b) || a==b);
	}

	public override string ToString()
	{
		return ToString(true);
	}

	public void ToString(bool writePort,out string dest)
	{
		dest=ToString(writePort);
	}

  public SystemAddress() : this(RakNetPINVOKE.new_SystemAddress__SWIG_0(), true) {
  }

  public SystemAddress(string str) : this(RakNetPINVOKE.new_SystemAddress__SWIG_1(str), true) {
  }

  public SystemAddress(string str, ushort port) : this(RakNetPINVOKE.new_SystemAddress__SWIG_2(str, port), true) {
  }

  public ushort debugPort {
    set {
      RakNetPINVOKE.SystemAddress_debugPort_set(swigCPtr, value);
    } 
    get {
      ushort ret = RakNetPINVOKE.SystemAddress_debugPort_get(swigCPtr);
      return ret;
    } 
  }

  public static int size() {
    int ret = RakNetPINVOKE.SystemAddress_size();
    return ret;
  }

  public static uint ToInteger(SystemAddress sa) {
    uint ret = RakNetPINVOKE.SystemAddress_ToInteger(SystemAddress.getCPtr(sa));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public byte GetIPVersion() {
    byte ret = RakNetPINVOKE.SystemAddress_GetIPVersion(swigCPtr);
    return ret;
  }

  public uint GetIPPROTO() {
    uint ret = RakNetPINVOKE.SystemAddress_GetIPPROTO(swigCPtr);
    return ret;
  }

  public void SetToLoopback() {
    RakNetPINVOKE.SystemAddress_SetToLoopback__SWIG_0(swigCPtr);
  }

  public void SetToLoopback(byte ipVersion) {
    RakNetPINVOKE.SystemAddress_SetToLoopback__SWIG_1(swigCPtr, ipVersion);
  }

  public bool IsLoopback() {
    bool ret = RakNetPINVOKE.SystemAddress_IsLoopback(swigCPtr);
    return ret;
  }

  public string ToString(bool writePort, char portDelineator) {
    string ret = RakNetPINVOKE.SystemAddress_ToString__SWIG_0(swigCPtr, writePort, portDelineator);
    return ret;
  }

  public string ToString(bool writePort) {
    string ret = RakNetPINVOKE.SystemAddress_ToString__SWIG_1(swigCPtr, writePort);
    return ret;
  }

  public void ToString(bool writePort, string dest, char portDelineator) {
    RakNetPINVOKE.SystemAddress_ToString__SWIG_2(swigCPtr, writePort, dest, portDelineator);
  }

  public bool FromString(string str, char portDelineator, int ipVersion) {
    bool ret = RakNetPINVOKE.SystemAddress_FromString__SWIG_0(swigCPtr, str, portDelineator, ipVersion);
    return ret;
  }

  public bool FromString(string str, char portDelineator) {
    bool ret = RakNetPINVOKE.SystemAddress_FromString__SWIG_1(swigCPtr, str, portDelineator);
    return ret;
  }

  public bool FromString(string str) {
    bool ret = RakNetPINVOKE.SystemAddress_FromString__SWIG_2(swigCPtr, str);
    return ret;
  }

  public bool FromStringExplicitPort(string str, ushort port, int ipVersion) {
    bool ret = RakNetPINVOKE.SystemAddress_FromStringExplicitPort__SWIG_0(swigCPtr, str, port, ipVersion);
    return ret;
  }

  public bool FromStringExplicitPort(string str, ushort port) {
    bool ret = RakNetPINVOKE.SystemAddress_FromStringExplicitPort__SWIG_1(swigCPtr, str, port);
    return ret;
  }

  public void CopyPort(SystemAddress right) {
    RakNetPINVOKE.SystemAddress_CopyPort(swigCPtr, SystemAddress.getCPtr(right));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool EqualsExcludingPort(SystemAddress right) {
    bool ret = RakNetPINVOKE.SystemAddress_EqualsExcludingPort(swigCPtr, SystemAddress.getCPtr(right));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public ushort GetPort() {
    ushort ret = RakNetPINVOKE.SystemAddress_GetPort(swigCPtr);
    return ret;
  }

  public ushort GetPortNetworkOrder() {
    ushort ret = RakNetPINVOKE.SystemAddress_GetPortNetworkOrder(swigCPtr);
    return ret;
  }

  public void SetPortHostOrder(ushort s) {
    RakNetPINVOKE.SystemAddress_SetPortHostOrder(swigCPtr, s);
  }

  public void SetPortNetworkOrder(ushort s) {
    RakNetPINVOKE.SystemAddress_SetPortNetworkOrder(swigCPtr, s);
  }

  public bool SetBinaryAddress(string str, char portDelineator) {
    bool ret = RakNetPINVOKE.SystemAddress_SetBinaryAddress__SWIG_0(swigCPtr, str, portDelineator);
    return ret;
  }

  public bool SetBinaryAddress(string str) {
    bool ret = RakNetPINVOKE.SystemAddress_SetBinaryAddress__SWIG_1(swigCPtr, str);
    return ret;
  }

  public void ToString_Old(bool writePort, string dest, char portDelineator) {
    RakNetPINVOKE.SystemAddress_ToString_Old__SWIG_0(swigCPtr, writePort, dest, portDelineator);
  }

  public void ToString_Old(bool writePort, string dest) {
    RakNetPINVOKE.SystemAddress_ToString_Old__SWIG_1(swigCPtr, writePort, dest);
  }

  public void FixForIPVersion(SystemAddress boundAddressToSocket) {
    RakNetPINVOKE.SystemAddress_FixForIPVersion(swigCPtr, SystemAddress.getCPtr(boundAddressToSocket));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
  }

  public bool IsLANAddress() {
    bool ret = RakNetPINVOKE.SystemAddress_IsLANAddress(swigCPtr);
    return ret;
  }

  public SystemAddress CopyData(SystemAddress input) {
    SystemAddress ret = new SystemAddress(RakNetPINVOKE.SystemAddress_CopyData(swigCPtr, SystemAddress.getCPtr(input)), false);
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public bool Equals(SystemAddress right) {
    bool ret = RakNetPINVOKE.SystemAddress_Equals(swigCPtr, SystemAddress.getCPtr(right));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private bool OpNotEqual(SystemAddress right) {
    bool ret = RakNetPINVOKE.SystemAddress_OpNotEqual(swigCPtr, SystemAddress.getCPtr(right));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private bool OpGreater(SystemAddress right) {
    bool ret = RakNetPINVOKE.SystemAddress_OpGreater(swigCPtr, SystemAddress.getCPtr(right));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  private bool OpLess(SystemAddress right) {
    bool ret = RakNetPINVOKE.SystemAddress_OpLess(swigCPtr, SystemAddress.getCPtr(right));
    if (RakNetPINVOKE.SWIGPendingException.Pending) throw RakNetPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public ushort systemIndex {
    set {
      RakNetPINVOKE.SystemAddress_systemIndex_set(swigCPtr, value);
    } 
    get {
      ushort ret = RakNetPINVOKE.SystemAddress_systemIndex_get(swigCPtr);
      return ret;
    } 
  }

}

}
