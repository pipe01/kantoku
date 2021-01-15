interface Window {
    chrome: Chrome;
}

declare var chrome: Chrome;

interface Chrome {
    sockets: Sockets;
}

interface Sockets {
    udp: Udp
}

interface Udp {
    /**
     * Binds the local address and port for the socket. For a client socket, it is recommended to use port 0 to let the platform pick a free port.
     * 
     * Once the bind operation completes successfully, onReceive events are raised when UDP packets arrive on the address/port specified -- unless the socket is paused.
     * @param socketId The socket ID.
     * @param address The address of the local machine. DNS name, IPv4 and IPv6 formats are supported. Use "0.0.0.0" to accept packets from all local available network interfaces.
     * @param port The port of the local machine. Use "0" to bind to a free port.
     * @param callback Called when the bind operation completes.
     */
    bind(socketId: number, address: string, port: number, callback: (result: number) => void);

    /**
     * Closes the socket and releases the address/port the socket is bound to. Each socket created should be closed after use. The socket id is no longer valid as soon at the function is called. However, the socket is guaranteed to be closed only when the callback is invoked.
     * @param socketId The socket ID.
     * @param callback Called when the close operation completes.
     */
    close(socketId: number, callback: () => void);

    /**
     * Creates a UDP socket.
     * @param callback Called when the socket has been created.
     */
    create(callback: (createInfo: CreateInfo) => void);

    /**
     * Creates a UDP socket with the given properties.
     * @param properties The socket properties.
     * @param callback Called when the socket has been created.
     */
    create(properties: SocketProperties, callback: (createInfo: CreateInfo) => void);

    /**
     * Retrieves the state of the given socket.
     * @param socketId The socket ID.
     * @param callback Called when the socket state is available.
     */
    getInfo(socketId: number, callback: (socketInfo: SocketInfo) => void);

    /**
     * Gets the multicast group addresses the socket is currently joined to.
     * @param socketId The socket ID.
     * @param callback Called with an array of strings of the result.
     */
    getJoinedGroups(socketId: number, callback: (groups: string[]) => void);

    /**
     * Retrieves the list of currently opened sockets owned by the application.
     * @param callback Called when the list of sockets is available.
     */
    getSockets(callback: (socketInfos: SocketInfo[]) => void);

    /**
     * Joins the multicast group and starts to receive packets from that group. The socket must be bound to a local port before calling this method.
     * @param socketId The socket ID.
     * @param address The group address to join. Domain names are not supported.
     * @param callback Called when the joinGroup operation completes.
     */
    joinGroup(socketId: number, address: string, callback: (result: number) => void);

    /**
     * Leaves the multicast group previously joined using joinGroup. This is only necessary to call if you plan to keep using the socketafterwards, since it will be done automatically by the OS when the socket is closed.
     * 
     * Leaving the group will prevent the router from sending multicast datagrams to the local host, presuming no other process on the host is still joined to the group.
     * @param socketId The socket ID.
     * @param address The group address to leave. Domain names are not supported.
     * @param callback Called when the leaveGroup operation completes.
     */
    leaveGroup(socketId: number, address: string, callback: (result: number) => void);

    /**
     * Sends data on the given socket to the given address and port. The socket must be bound to a local port before calling this method.
     * @param socketId The socket ID.
     * @param data The data to send.
     * @param address The address of the remote machine.
     * @param port The port of the remote machine.
     * @param callback Called when the send operation completes.
     */
    send(socketId: number, data: ArrayBuffer, address: string, port: number, callback: (sendInfo: SendInfo) => void);

    /**
     * Enables or disables broadcast packets on this socket.
     * @param socketId The socket ID.
     * @param enabled true to enable broadcast packets, false to disable them.
     * @param callback Callback from the setBroadcast method.
     */
    setBroadcast(socketId: number, enabled: boolean, callback: (result: number) => void);

    /**
     * Sets whether multicast packets sent from the host to the multicast group will be looped back to the host.
     * 
     * Note: the behavior of setMulticastLoopbackMode is slightly different between Windows and Unix-like systems. The inconsistency happens only when there is more than one application on the same host joined to the same multicast group while having different settings on multicast loopback mode. On Windows, the applications with loopback off will not RECEIVE the loopback packets; while on Unix-like systems, the applications with loopback off will not SEND the loopback packets to other applications on the same host. See MSDN: http://goo.gl/6vqbj
     * 
     * Calling this method does not require multicast permissions.
     * @param socketId The socket ID.
     * @param enabled Indicate whether to enable loopback mode.
     * @param callback Called when the configuration operation completes.
     */
    setMulticastLoopbackMode(socketId: number, enabled: boolean, callback: (result: number) => void);

    /**
     * Sets the time-to-live of multicast packets sent to the multicast group.
     * 
     * Calling this method does not require multicast permissions.
     * @param socketId The socket ID.
     * @param ttl The time-to-live value.
     * @param callback Called when the configuration operation completes.
     */
    setMulticastTimeToLive(socketId: number, ttl: number, callback: (result: number) => void);

    /**
     * Pauses or unpauses a socket. A paused socket is blocked from firing onReceive events.
     * @param socketId The socket ID.
     * @param paused Flag to indicate whether to pause or unpause.
     * @param callback Called when the socket has been successfully paused or unpaused.
     */
    setPaused(socketId: number, paused: boolean, callback: () => void);

    /**
     * Updates the socket properties.
     * @param socketId The socket ID.
     * @param properties The properties to update.
     * @param callback Called when the properties are updated.
     */
    update(socketId: number, properties: SocketProperties, callback: () => void);

    /**
     * Event raised when a UDP packet has been received for the given socket.
     */
    onReceive: ChromeEventTarget<(info: ReceiveInfo) => void>;

    /**
     * Event raised when a network error occured while the runtime was waiting for data on the socket address and port. Once this event is raised, the socket is paused and no more onReceive events will be raised for this socket until the socket is resumed.
     */
    onReceiveError: ChromeEventTarget<(info: ReceiveErrorInfo) => void>;
}

interface CreateInfo {
    /**
     * The ID of the newly created socket. Note that socket IDs created from this API are not compatible with socket IDs created from other APIs, such as the deprecated socket API.
     */
    socketId: number;
}

interface ReceiveErrorInfo {
    /**
     * The result code returned from the underlying recvfrom() call.
     */
    resultCode: number;

    /**
     * The socket ID.
     */
    socketId: number;
}

interface ReceiveInfo {
    /**
     * The UDP packet content (truncated to the current buffer size).
     */
    data: ArrayBuffer;

    /**
     * The address of the host the packet comes from.
     */
    remoteAddress: string;

    /**
     * The port of the host the packet comes from.
     */
    remotePort: number;

    /**
     * The socket ID.
     */
    socketId: number;
}

interface SendInfo {
    /**
     * The number of bytes sent (if result == 0)
     */
    bytesSent?: number;

    /**
     * The result code returned from the underlying network call. A negative value indicates an error.
     */
    resultCode: number;
}

interface SocketInfo {
    /**
     * The size of the buffer used to receive data. If no buffer size has been specified explictly, the value is not provided.
     */
    bufferSize?: number;

    /**
     * If the underlying socket is bound, contains its local IPv4/6 address.
     */
    localAddress?: string;

    /**
     * If the underlying socket is bound, contains its local port.
     */
    localPort: number;

    /**
     * Application-defined string associated with the socket.
     */
    name?: string;

    /**
     * Flag indicating whether the socket is blocked from firing onReceive events.
     */
    paused: boolean;

    /**
     * Flag indicating whether the socket is left open when the application is suspended (see SocketProperties.persistent).
     */
    persistent: boolean;

    /**
     * The socket identifier.
     */
    socketId: number;
}

interface SocketProperties {
    /**
     * The size of the buffer used to receive data. If the buffer is too small to receive the UDP packet, data is lost. The default value is 4096.
     */
    bufferSize?: number;

    /**
     * An application-defined string associated with the socket.
     */
    name?: string;

    /**
     * Flag indicating if the socket is left open when the event page of the application is unloaded. The default value is "false." When the application is loaded, any sockets previously opened with persistent=true can be fetched with getSockets.
     */
    persistent?: boolean;
}

interface ChromeEventTarget<TFunc> {
    addListener(listener: TFunc);
    removeListener(listener: TFunc);
}
