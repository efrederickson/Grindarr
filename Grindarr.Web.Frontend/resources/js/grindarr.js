// This module creates an easy JS api for interacting with the Grindarr REST API

var grindarr = (function () {
    return {
        WEB_ROOT: '/api/',
        ENDPOINT_DOWNLOAD_INDEX: 'download',
        ENDPOINT_DOWNLOAD_GET: 'download/',
        ENDPOINT_DOWNLOAD_CREATE: 'download',
        ENDPOINT_DOWNLOAD_UPDATE: 'download/',
        ENDPOINT_DOWNLOAD_DELETE: 'download/',

        ENDPOINT_CONFIG_LIST: 'config',
        ENDPOINT_CONFIG_UPDATE: 'config',

        ENDPOINT_SCRAPERS_LIST: 'scraper',
        ENDPOINT_SCRAPERS_AVAILABLE: 'scraper/available',
        ENDPOINT_SCRAPERS_GET: 'scraper/',
        ENDPOINT_SCRAPER_ADD: 'scraper',
        ENDPOINT_SCRAPER_DELETE: 'scraper/',

        API_KEY: "no-api-key-set",

        DownloadStatus: {
            Pending: 0,
            Paused: 1,

            Downloading: 2,

            Completed: 3,
            Failed: 4,
            Canceled: 5
        },

        authenticatedRequest: function (url, data, callback, method = "GET", synchronous = false) {
            if (synchronous) {
                var res = null;
                $.ajax({
                    url: url + "?apikey=" + grindarr.API_KEY,
                    type: method,
                    async: false,
                    //dataType: 'json',
                    contentType: 'application/json',
                    data: data == null ? "" : JSON.stringify(data)
                }).done(function (data) {
                    res = data;
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    console.log("Request failed: " + textStatus);
                });
                return res;
            } else {
                $.ajax({
                    url: url + "?apikey=" + grindarr.API_KEY,
                    type: method,
                    //dataType: 'json',
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
            }
        },

        // DOWNLOADS

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
        },

        // CONFIG

        getConfig: function () {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_CONFIG_LIST, null, null, method = "GET", synchronous = true);
        },

        updateConfigField: function (field, value, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_CONFIG_UPDATE, {
                [field]: value // Map value of 'field' to 'value'
            }, cb, method = "PUT");
        },

        // CONFIG FIELDS

        config: {
            getInProgressDownloadsFolder: function () {
                return grindarr.getConfig().inProgressDownloadsFolder;
            },

            getCompletedDownloadsFolder: function () {
                return grindarr.getConfig().completedDownloadsFolder;
            },

            getIgnoreStalledDownloads: function () {
                return grindarr.getConfig().ignoreStalledDownloads;
            },

            getStalledDownloadCutoff: function () {
                return grindarr.getConfig().stalledDownloadCutoff;
            },

            setInProgressDownloadsFolder: function (path) {
                return grindarr.updateConfigField("inProgressDownloadsFolder", path);
            },

            setCompletedDownloadsFolder: function (path) {
                return grindarr.updateConfigField("completedDownloadsFolder", path);
            },

            setIgnoreStalledDownloads: function (val) {
                val = !!val; // convert to boolean
                return grindarr.updateConfigField("ignoreStalledDownloads", val);
            },

            setStalledDownloadCutoff: function (cutoff) {
                if (cutoff < 0) // Sanitize value (yes, i know client-side validation is not the best, this is also done on the server)
                    cutoff = 0;
                return grindarr.updateConfigField("stalledDownloadCutoff", cutoff);
            },

        },

        // POST PROCESSOR

        // SCRAPER
        scrapers: {
            cached_scraper_lookup: {},

            parseName: function (assemblyName) {
                if (grindarr.scrapers.cached_scraper_lookup[assemblyName]) {
                    return grindarr.scrapers.cached_scraper_lookup[assemblyName];
                }
                var res = assemblyName.substring(0, assemblyName.indexOf(','));
                res = res.substring(res.lastIndexOf('.') + 1);
                return res;
            },

            getScrapers: function () {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPERS_LIST, null, null, method = "GET", synchronous = true);
            },

            getAvailableScrapers: function () {
                var res = grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPERS_AVAILABLE, null, null, method = "GET", synchronous = true);
                var resMapped = [];
                $(res).each(function (k, v) {
                    var parsedAssemblyName = grindarr.scrapers.parseName(v);
                    grindarr.scrapers.cached_scraper_lookup[parsedAssemblyName] = v;
                    resMapped.push(parsedAssemblyName);
                });
                return resMapped;
            },

            getScraper: function (id) {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPERS_GET + id, null, null, method = "GET", synchronous = true);
            },

            configureScraper: function (name, args) {

            },

            deleteScraper: function (id) {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPER_DELETE + id, null, null, method = "DELETE", synchronous = true);
            },
        },
    }
})();