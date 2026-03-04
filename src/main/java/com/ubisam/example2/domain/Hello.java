package com.ubisam.example2.domain;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;
import lombok.Data;

//ORM - Object Relation Mapping
@Entity
@Data
@Table(name = "example_hello") //테이블 생성
public class Hello {

    @Id
    @GeneratedValue
    private Long id;
    // @EmbeddedId
    // private Id id;
    private String name;
    private String email;

    //keyword는 DB에 저장되지 않는 필드
    @Transient
    private String keyword;

    @Transient
    private Long helloId;
    // @ManyToOne
    // private Hello hello;

    // @Data
    // @Embeddable
    // public class Id{
    //     private String id1;
    //     private String id2;
    // }


}
