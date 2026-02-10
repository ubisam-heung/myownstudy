package com.ubisam.example2.helloes;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.Data;

//ORM - Object Relation Mapping
@Entity
@Data
@Table(name = "t_str") //테이블 생성
public class Hello {

    @Id
    @GeneratedValue
    private Long id;
    // @EmbeddedId
    // private Id id;
    private String name;
    private String email;

    // @Data
    // @Embeddable
    // public class Id{
    //     private String id1;
    //     private String id2;
    // }


}
