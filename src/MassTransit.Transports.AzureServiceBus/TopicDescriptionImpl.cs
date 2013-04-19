// Copyright 2012 Henrik Feldt
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

#pragma warning disable 1591

namespace MassTransit.Transports.AzureServiceBus
{
    using System;
    using System.Runtime.Serialization;


    /// <summary>
    /// See <see cref="TopicDescription"/>
    /// </summary>
    public class TopicDescriptionImpl : TopicDescription
    {
        readonly Microsoft.ServiceBus.Messaging.TopicDescription _description;

        public TopicDescriptionImpl(string path)
        {
            _description = new Microsoft.ServiceBus.Messaging.TopicDescription(path);
        }

        public bool Equals(TopicDescription other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(other.Path, Path);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as TopicDescription);
        }

        public int CompareTo(TopicDescription other)
        {
            if (other == null)
                return 1;
            return string.CompareOrdinal(Path, other.Path);
        }

        #region Delegating Members

        public bool IsReadOnly
        {
            get { return _description.IsReadOnly; }
        }

        public ExtensionDataObject ExtensionData
        {
            get { return _description.ExtensionData; }
        }

        public TimeSpan DefaultMessageTimeToLive
        {
            get { return _description.DefaultMessageTimeToLive; }
        }

        public long MaxSizeInMegabytes
        {
            get { return _description.MaxSizeInMegabytes; }
        }

        public bool RequiresDuplicateDetection
        {
            get { return _description.RequiresDuplicateDetection; }
        }

        public TimeSpan DuplicateDetectionHistoryTimeWindow
        {
            get { return _description.DuplicateDetectionHistoryTimeWindow; }
        }

        public long SizeInBytes
        {
            get { return _description.SizeInBytes; }
        }

        public string Path
        {
            get { return _description.Path; }
        }

        public bool EnableBatchedOperations
        {
            get { return _description.EnableBatchedOperations; }
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (!(obj is TopicDescription))
                return false;
            return Equals((TopicDescription)obj);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("TopicDescription={{ Path:'{0}' }}", Path);
        }
    }
}