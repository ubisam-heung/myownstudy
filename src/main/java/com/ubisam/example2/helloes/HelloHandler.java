package com.ubisam.example2.helloes;

import org.springframework.data.rest.core.annotation.HandleAfterCreate;
import org.springframework.data.rest.core.annotation.HandleBeforeCreate;
import org.springframework.data.rest.core.annotation.RepositoryEventHandler;
import org.springframework.stereotype.Component;

@Component
@RepositoryEventHandler
public class HelloHandler {

    //중간을 캐치
    @HandleBeforeCreate
    public void beforeCreate(Hello hello){

        //강제로 데이터를 세팅
        // hello.setName("abc");

        //역할 제한
        // throw new RuntimeException();


        System.out.println("[beforeCreate] testtesttesttesttesttesttesttest");
    }

    @HandleAfterCreate
    public void afterCreate(Hello hello){
        //Auditing(추적)
        System.out.println("[afterCreate] testtesttesttesttesttesttesttest");
    }
}
