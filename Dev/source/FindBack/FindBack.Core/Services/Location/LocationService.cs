﻿namespace FindBack.Core.Services.Location
{
    using Cirrious.CrossCore;
    using Cirrious.MvvmCross.Plugins.Location;
    using Cirrious.MvvmCross.Plugins.Messenger;

    public class LocationService : ILocationService
    {
        private readonly IMvxLocationWatcher _locationWatcher;
        private readonly IMvxMessenger _messenger;
        private readonly object _lockObject = new object();
        private MvxGeoLocation _latestLocation;

        public LocationService(IMvxLocationWatcher locationWatcher, IMvxMessenger messenger)
        {
            this._locationWatcher = locationWatcher;
            this._messenger = messenger;
            this._locationWatcher.Start(new MvxLocationOptions(), this.OnLocation, this.OnLocationError);
        }

        private void OnLocationError(MvxLocationError error)
        {
            Mvx.Error("Error in retrieving location {0}", error.Code);
        }

        private void OnLocation(MvxGeoLocation location)
        {
            lock (_lockObject)
            {
                _latestLocation = location;
            }

            var message = new LocationMessage(this,
                                location.Coordinates.Latitude,
                                location.Coordinates.Longitude);

            _messenger.Publish(message);
        }

        public bool TryGetLatestLocation(out double? lat, out double? lng)
        {
            lock (_lockObject)
            {
                if (_latestLocation == null)
                {
                    lat = lng = null;
                    return false;
                }

                lat = _latestLocation.Coordinates.Latitude;
                lng = _latestLocation.Coordinates.Longitude;
                return true;
            }
        }
    }
}