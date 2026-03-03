package com.ubisam.example2.Worlds;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.Data;

@Entity
@Data
@Table(name = "t_str") //테이블 생성
public class World {

    @Id
    @GeneratedValue
    private Long id;
    private String name;
    private String email;
    
}
