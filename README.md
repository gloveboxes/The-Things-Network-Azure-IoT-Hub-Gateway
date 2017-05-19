# The-Things-Network-Azure-IoT-Hub-Gateway
The Azure Function App bridges The Things Network and Azure IoT Hub networks

# The Things Network To Azure IoT Hub Gateway

This diagram outlines how the gateway works. But in summary:-

1. The HTTP POST request from The Things Networks triggers the Azure Function App Gateway.
2. The Gateway validates The Things Network Application Id
3. The Gateway gets the device key (matched by device id) from the Azure IoT Hub Device Registry.
4. An Azure IoT Hub SaS Token is generated using the device key.
5. The Things Network payload is readied for Azure IoT Hub
    * The raw Things Network payload is transformed to a JSON payload.
    * An Azure IoT Hub Routing HTTP Header is added named "route-id" whose value is the name of the Things Network Application prefixed with "ttn-" 
6. The data is sent to Azure IoT Hub. 

![The Things Network](https://raw.githubusercontent.com/gloveboxes/The-Things-Network-Azure-IoT-Hub-Gateway/master/Resources/Architecture.JPG)


# Gateway App Settings

The following Azure Function App app setting keys are required


|Key|Value|
|-----|------|
|IoTHubHostname| youriothub.azure-devices.net|
|IotHubRegistryReadPolicyKeyName|Default name is registryRead |
|IotHubRegistryReadPolicyKey| Your registry read key|
|TTNAppIDsCommaSeperated| Comma separated list of The Things Network application names that will use this gateway|




# Gateway Configuration


This The Things Network Azure IoT Hub Gateway works in conjunction with a Things Networks HTTP Integration.

You need the URL address of the Azure Function. From the Azure Portal, open the Things Network function and click in the "Get function URL" and copy the link.

![Azure Function App Http Address](https://raw.githubusercontent.com/gloveboxes/The-Things-Network-Azure-IoT-Hub-Gateway/master/Resources/ThingsNetworkBridgeFunctionAppHttpTrigger.JPG)



# The Things Network Application HTTP Integration Configuration

From The Things Network Application Console create a new HTTP Integration and paste the URL you copied from the Azure Function App.

![Things Network Integration](https://raw.githubusercontent.com/gloveboxes/The-Things-Network-Azure-IoT-Hub-Gateway/master/Resources/TheThingsNetworkHttpIntegration.JPG)





# Telemetry Schema

You need to modify the Telemetry Class to match the shape of the data passed from The Things Network

```c#
using Newtonsoft.Json;
using System;

namespace TheThingsNetworkGateway
{
    public class Telemetry
    {
        public UInt32 Level { get; set; }
        public string Schema { get; set; } = "1";
        public string ToJson(UInt32 level)
        {
            this.Level = level;
            return JsonConvert.SerializeObject(this);
        }
    }
}

```
