using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayServiceWrapper
{
    private UnityTransport _transport;

    public RelayServiceWrapper()
    {
        _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public async Task<string> SetupHostRelayAsync(int maxPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            _transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError("SetupHostRelay failed: " + e);
            throw;
        }
    }

    public async Task SetupClientRelayAsync(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            _transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
        }
        catch (Exception e)
        {
            Debug.LogError("SetupClientRelay failed: " + e);
            throw;
        }
    }
}
