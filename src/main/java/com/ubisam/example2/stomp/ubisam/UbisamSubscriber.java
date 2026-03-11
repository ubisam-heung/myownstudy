package com.ubisam.example2.stomp.ubisam;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;

import com.ubisam.example2.api.helloes.HelloRepository;
import com.ubisam.example2.domain.Hello;

import io.u2ware.common.stomp.client.WebsocketStompClient;
import io.u2ware.common.stomp.client.WebsocketStompClientHandler;
import io.u2ware.common.stomp.client.config.WebsocketStompProperties;

@Component
public class UbisamSubscriber implements WebsocketStompClientHandler{

    @Autowired
    public WebsocketStompProperties properties;

    @Autowired
    public ObjectMapper mapper;

    @Autowired
    public HelloRepository helloRepository;

    @Override
    public void handleFrame(WebsocketStompClient client, JsonNode message) {
        System.out.println("message: " + message.toString());

        ObjectNode data = mapper.createObjectNode();
        data.put("message", "hello");

        Hello h = new Hello();
        h.setEmail(message.has("principal") ? message.get("principal").asText() : null);
        h.setName(message.has("payload") ? message.get("payload").asText() : null);
        helloRepository.save(h);

        client.send("/app/robot", data);
    }

    @Override
    public String getDestination() {
        return properties.getSubscriptions().get("ubisam");
    }
}
