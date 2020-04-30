# Grindarr

This project is designed to provide a capability similar to Torrents or Usenet for DDL (direct downloads) along with "indexing" (via search)

Still needs work, but the base functionality seems to work. 

The project provides two main portions: the REST API backend and the web frontend which provides a basic live frontend for the backend. 
The backend is securable with an API key, however the default is to not enforce the API key. 
The web portion should be reverse-proxied through something that can provide security such as HSTS, certs, brute force suppression, etc. or kept private.  

## Project status:

Downloaders:

- [ ] Dropbox
- [ ] MEGA
- [ ] Mediafire
- [X] Zippyshare
- [X] Generic (just downloads the link)

Scrapers (Search):

- [X] Generic Apache open directory
- [X] Generic Nginx open directory
- [X] getcomics.info

Web APIs:

- [X] custom REST API (documentation TODO)
- [ ] newznab-like API 

Core:

- [X] Pausable/Resumable downloads
- [ ] Pause download without closing web client (causing single-use URLs to fail)
- [ ] Resume download without wiping incomplete file
- [ ] Provide RSS for monitoring new/changed entries without manually re-searching specific terms
- [ ] Handle failed scrapers without causing the entire search to fail (e.g. an open directory goes down)

## Project Design

Basically how the system works is like this:

A `scraper` will return a list of `ContentItem`s which contain meta data about an item (like the title, date, download links). 
The `ContentItem`s are then returned to the client and ones that are desired to be downloaded can be `POST`ed to the download endpoint, 
where they will be turned into a `DownloadItem`, where the best download link from the list provided will be selected and added to the queue. 
The download link selected is based off of matching the domain against the registered downloaders (e.g. MEGA, Dropbox). 
The download will commence, and if successful, a series of `IPostProcessor`s will be run against, doing things like moving it to the completed folder, etc. 

I have tried to use the latest C# technologies available, including the use of `await foreach`, which I think is a really great addition to the language. 

## Building/Running

Uses .NET Core / ASP.NET Core + Docker, so `dotnet build` / `dotnet run` should do the trick. 
If I don't abandon this, eventually there will be a Docker image. 
