# The-Things-Network-Azure-IoT-Hub-Gateway
The Azure Function App bridges The Things Network and Azure IoT Hub networks

# The Things Network To Azure IoT Hub Gateway

![The Things Network](https://raw.githubusercontent.com/gloveboxes/The-Things-Network-Azure-IoT-Hub-Gateway/master/Resources/Architecture.JPG)

This diagram outlines how the gateway works. But in summary:-

1. The HTTP POST request from The Things Networks triggers the Azure Function App Gateway.
2. The Gateway validates The Things Network Application Id
3. The Gateway gets the device key (matched by device id) from the Azure IoT Hub Device Registry.
4. An Azure IoT Hub SaS Token is generated using the device key.
5. The Things Network payload is readied for Azure IoT Hub
    * The raw Things Network payload is transformed to a JSON payload.
    * An Azure IoT Hub Routing HTTP Header is added named "route-id" whose value is the name of the Things Network Application prefixed with "ttn-" 
6. The data is sent to Azure IoT Hub. 

# How to Deploy the Azure Function App to Azure

This outlines the process to publish the gateway solution to Azure.

1. On the Azure Portal create a Function App
2. Download the Publish Profile
3. Clone this solution
4. Open with Visual Studio 2017 (Community Edition is fine)
4. Modify the Telemetry class to match your requirements
5. Publish the solution to Azure by right mouse clicking on the Project.
6. From Publish dialogue target and import the Publish Profile you downloaded
7. Create New
8. Publish




## Telemetry Schema

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


## Gateway App Settings

The following Azure Function App Application Setting Keys/Value pairs are required


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






