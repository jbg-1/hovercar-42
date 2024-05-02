using System.Collections;
using System.Collections.Generic;
using MQTTnet;
using Newtonsoft.Json;
using PuzzleCubes.Communication;
using PuzzleCubes.Controller;
using UnityEngine;

public class HelloCubesEventDispatcher : EventDispatcher
{
   public HelloCubesEvent helloCubesEvent;

   public const string helloCubesRequestTopic =  "puzzleCubes/app/helloCubes";                          // from server 
   public string helloCubesResponseTopic(string cubeId) =>  $"puzzleCubes/{cubeId}/app/helloCubes";                 // to server

    

    protected override void Initialize()
    {
        base.Initialize();

      


        subscriptions.Add(new MqttTopicFilterBuilder().WithTopic(helloCubesRequestTopic).Build() ,HandleHelloCubes);
    }

    // SAMPLE HANDLER FOR MQTT MESSAGE
    public void HandleHelloCubes(MqttApplicationMessage msg, IList<string> wildcardItem){
         var data = System.Text.Encoding.UTF8.GetString(msg.Payload);
        var result = JsonConvert.DeserializeObject<HelloCubes>(data);
        helloCubesEvent.Invoke( result);
    }


    public void DispatchHelloCubes(HelloCubes hc)
    {
        Debug.Log("DispatchHelloCubes");       
        var json = JsonConvert.SerializeObject(hc, Formatting.Indented, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects
        });
        // this.SendZmq(json, true);  
        var msg = new MqttApplicationMessage();
        msg.Topic = helloCubesResponseTopic(AppController.Instance.state.CubeId);
        msg.Payload = System.Text.Encoding.UTF8.GetBytes(json);
        msg.MessageExpiryInterval = 3600;
        
        
        this.mqttCommunication.Send(msg); 
        
        
    }


}
