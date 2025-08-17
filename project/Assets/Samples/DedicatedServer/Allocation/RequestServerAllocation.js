// Cloud Code JavaScript script
// Name this file: RequestServerAllocation.js

// Cloud Code exposes the Multiplay client here:
const { AllocationsApi } = require("@unity-services/multiplay-1.0");

// Optional: tiny helper to sleep during polling
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

/**
 * Allocates a Multiplay (Game Server Hosting) server and (optionally) waits for IP/port.
 *
 * params:
 *  - fleetId: string (required)
 *  - buildConfigurationId: number|string (required)
 *  - regionId: string (required) e.g., "us-west1"
 *  - payload: object (optional) will be JSON-stringified and delivered to the server
 *  - waitForReady: boolean (optional, default true) — if false, returns allocationId immediately
 *  - maxPolls: number (optional, default 10)
 *  - pollDelayMs: number (optional, default 2000)
 */
module.exports = async ({ params, context }) => {
  const {
    fleetId,
    buildConfigurationId,
    regionId,
    payload,
    waitForReady = true,
    maxPolls = 10,
    pollDelayMs = 2000,
  } = params || {};

  if (!fleetId || !buildConfigurationId || !regionId) {
    throw new Error("fleetId, buildConfigurationId, and regionId are required.");
  }

  // Cloud Code SDK: https://cloud-code-sdk-documentation.cloud.unity3d.com/multiplay/v1.0
  // Available libraries list (shows import path & classes):
  // https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/scripts/reference/available-libraries
  const allocations = new AllocationsApi();

  // BuildConfig must be a number per model; cast if string
  const buildConfigIdNum = typeof buildConfigurationId === "string"
      ? parseInt(buildConfigurationId, 10)
      : buildConfigurationId;

  const allocateReq = {
    buildConfigurationId: buildConfigIdNum,
    regionId,
    // The API will return an allocationId; you do NOT need to supply one.
    // Include a small JSON payload if you want to pass session config/allowlist/etc.
    ...(payload ? { payload: JSON.stringify(payload) } : {}),
  };

  // Queue the allocation (returns { allocationId, href })
  const allocateResp = await allocations.processAllocation(
      context.projectId,
      context.environmentId,
      fleetId,
      allocateReq
  );

  // Cloud Code SDK methods return AxiosResponse<T>; data holds the shape described in docs.
  const { allocationId, href } = allocateResp.data;

  if (!waitForReady) {
    return { allocationId, href };
  }

  // Poll for ready server details (ipv4 / gamePort).
  // Model: MultiplayAllocationsAllocation has fields ipv4, gamePort, ready, etc.
  // https://cloud-code-sdk-documentation.cloud.unity3d.com/multiplay/v1.0/multiplayallocationsallocation
  let last = null;
  for (let i = 0; i < maxPolls; i++) {
    const statusResp = await allocations.getAllocation(
        context.projectId,
        context.environmentId,
        fleetId,
        allocationId
    );
    last = statusResp.data;

    if (last && last.ipv4 && last.gamePort) {
      return {
        allocationId,
        server: {
          ip: last.ipv4,
          port: last.gamePort,
        },
        meta: {
          regionId: last.regionId,
          readyAt: last.ready,
          buildConfigurationId: last.buildConfigurationId,
          serverId: last.serverId,
        },
      };
    }

    await sleep(pollDelayMs);
  }

  // If we reach here, we never observed ipv4/gamePort.
  // Return the allocationId so the client can poll later.
  return {
    allocationId,
    pending: true,
    href,
    lastObserved: last || null,
    note: `Server not ready after ${maxPolls} polls @ ${pollDelayMs}ms. Client should poll again.`,
  };
};
