using System;
using System.Net.NetworkInformation;


namespace sharedrasil {
    public static class Connection {
        public static Boolean Check() {
            Ping ping = new Ping();
            string host = "google.com";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();

            try {
                PingReply reply = ping.Send(host, timeout, buffer, pingOptions);

                if (reply.Status == IPStatus.Success) {
                    return true;
                } else {
                    return false;
                }
            } catch {
                return false;
            }

        }
    }
}