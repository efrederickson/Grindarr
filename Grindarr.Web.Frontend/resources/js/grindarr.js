// This module creates an easy JS api for interacting with the Grindarr REST API

var grindarr = (function () {
    return {
        WEB_ROOT: '/api/',
        ENDPOINT_DOWNLOAD_INDEX: 'download',
        ENDPOINT_DOWNLOAD_GET: 'download/',
        ENDPOINT_DOWNLOAD_CREATE: 'download',
        ENDPOINT_DOWNLOAD_UPDATE: 'download/',
        ENDPOINT_DOWNLOAD_DELETE: 'download/',
        API_KEY: "no-api-key-set",

        DownloadStatus: {
            Pending: 0,
            Paused: 1,

            Downloading: 2,

            Completed: 3,
            Failed: 4,
            Canceled: 5
        },

        authenticatedRequest: function (url, data, callback, method = "GET") {
            $.ajax({
                url: url + "?apikey=" + grindarr.API_KEY,
                type: method,
                dataType: 'json',
                contentType: 'application/json',
                data: data == null ? "" : JSON.stringify(data)
            }).done(function (data) {
                if (callback != null) {
                    callback(data);
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                console.log("Request failed: " + textStatus);
                if (callback != null) {
                    callback(null);
                }
            });
        },

        getDownloads: function (cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_INDEX, null, cb);
        },

        getDownload: function (id, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_GET + id, null, cb);
        },

        cancelDownload: function (id, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_DELETE + id, null, cb, method = "DELETE");
        },

        updateDownload: function (id, newData, cb) {
            console.log("implement me");
        },

        createDownload: function (data, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_CREATE, data, cb, method = "POST");
        }
    }
})();