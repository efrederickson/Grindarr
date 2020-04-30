// This module creates an easy JS api for interacting with the Grindarr REST API

var grindarr = (function () {
    return {
        WEB_ROOT: '/api/',

        ENDPOINT_ACTIONS_SEARCH: 'actions/search/',

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

        ENDPOINT_POSTPROCESSOR_LIST: 'postprocessor',
        ENDPOINT_POSTPROCESSOR_GET: 'postprocessor/',
        ENDPOINT_POSTPROCESSOR_UPDATE: 'postprocessor/',

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
                    console.log("Request failed: " + jqXHR.responseText);
                    if (callback != null) {
                        callback(null, {
                            errorMessage: jqXHR.responseText,
                            success: false
                        });
                    }
                });
            }
        },

        textForDownloadStatus: function (status) {
            return Object.keys(grindarr.DownloadStatus).find(key => grindarr.DownloadStatus[key] == status);
        },

        bytesToHumanReadableString: function (bytes, si) {
            var thresh = si ? 1000 : 1024;
            if (Math.abs(bytes) < thresh) {
                return bytes + ' B';
            }
            var units = si
                ? ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB']
                : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];
            var u = -1;
            do {
                bytes /= thresh;
                ++u;
            } while (Math.abs(bytes) >= thresh && u < units.length - 1);
            return bytes.toFixed(1) + ' ' + units[u];
        },

        // DOWNLOADS

        getDownloads: function (cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_INDEX, null, cb);
        },

        getDownload: function (id, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_GET + id, null, cb);
        },

        cancelDownload: function (id, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_DELETE + id, null, cb, "DELETE");
        },

        updateDownload: function (id, newData, cb) {
            console.log("implement me");
        },

        createDownload: function (data, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_DOWNLOAD_CREATE, data, cb, "POST");
        },

        pauseDownload: function (id, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + "download/" + id + "/pause", null, cb, "POST");
        },
        
        resumeDownload: function (id, cb) {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + "download/" + id + "/resume", null, cb, "POST");
        },

        // CONFIG

        getConfig: function () {
            return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_CONFIG_LIST, null, null, "GET", true);
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

            setInProgressDownloadsFolder: function (path, cb) {
                return grindarr.updateConfigField("inProgressDownloadsFolder", path, cb);
            },

            setCompletedDownloadsFolder: function (path, cb) {
                return grindarr.updateConfigField("completedDownloadsFolder", path, cb);
            },

            setIgnoreStalledDownloads: function (val, cb) {
                val = !!val; // convert to boolean
                return grindarr.updateConfigField("ignoreStalledDownloads", val, cb);
            },

            setStalledDownloadCutoff: function (cutoff, cb) {
                if (cutoff < 0) // Sanitize value (yes, i know client-side validation is not the best, this is also done on the server)
                    cutoff = 0;
                return grindarr.updateConfigField("stalledDownloadCutoff", cutoff, cb);
            },

        },

        // POST PROCESSOR
        postProcessors: {
            getPostProcessors: function () {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_POSTPROCESSOR_LIST, null, null, method = "GET", true);
            },

            getPostProcessor: function (id) {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_POSTPROCESSOR_GET + id, null, null, method = "GET", true);
            },

            updatePostProcessor: function (id, enabled, cb) {
                var newOptions = {
                    "enabled": enabled
                };

                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_POSTPROCESSOR_UPDATE + id, newOptions, cb, "PATCH");
            }
        },

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
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPERS_LIST, null, null, method = "GET", true);
            },

            getAvailableScrapers: function () {
                var res = grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPERS_AVAILABLE, null, null, method = "GET", true);
                var resMapped = [];
                $(res).each(function (k, v) {
                    var parsedAssemblyName = grindarr.scrapers.parseName(v);
                    grindarr.scrapers.cached_scraper_lookup[parsedAssemblyName] = v;
                    resMapped.push(parsedAssemblyName);
                });
                return resMapped;
            },

            getScraper: function (id) {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPERS_GET + id, null, null, method = "GET", true);
            },

            configureScraper: function (name, args, cb) {
                var mappedArgs = {
                    className: grindarr.scrapers.cached_scraper_lookup[name] ?? name,
                    arguments: args
                };
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPER_ADD, mappedArgs, cb, "POST");
            },

            deleteScraper: function (id) {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_SCRAPER_DELETE + id, null, null, "DELETE", true);
            },
        },

        actions: {
            search: function (query, cb) {
                return grindarr.authenticatedRequest(grindarr.WEB_ROOT + grindarr.ENDPOINT_ACTIONS_SEARCH + query, null, cb, "POST");
            }
        },
    }
})();