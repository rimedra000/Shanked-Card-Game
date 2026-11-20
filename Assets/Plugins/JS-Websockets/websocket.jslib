var LibraryWebsocket = {
    $WebSocketClient: {
        s : null,
        callback: null,
    },

    OpenWebSocket : function (utf8str,callback,openCallBack,closeCallBack) {
        var str = UTF8ToString(utf8str);
        WebSocketClient.callback=callback
        WebSocketClient.s = new WebSocket(str);
        // console.log(str);
        WebSocketClient.s.onmessage = (event) => {event.data.bytes().then((v) => {
            var buffer = _malloc(v.length)
            HEAPU8.set(v, buffer/* >> 2*/)
            {{{ makeDynCall('vii', 'WebSocketClient.callback') }}}(buffer,v.length)
            _free(buffer)
        }); }
        WebSocketClient.s.onopen= () => { 
            {{{ makeDynCall('v','openCallBack')}}}()
        };
        WebSocketClient.s.onclose= () => { 
            {{{ makeDynCall('v','closeCallBack')}}}()
        };
    },

    CloseWebSocket : function () {
        WebSocketClient.s.close();
    },

    WebSocketSend : function (array, size){
        WebSocketClient.s.send(new Blob([HEAPU8.subarray(array, array + size)]));
    }
};

autoAddDeps(LibraryWebsocket, '$WebSocketClient');
mergeInto(LibraryManager.library, LibraryWebsocket);