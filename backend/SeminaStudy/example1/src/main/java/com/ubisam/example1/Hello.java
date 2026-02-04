package com.ubisam.example1;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.SequenceGenerator;
import jakarta.persistence.Table;
import lombok.Data;

@Entity
@Data
@Table(name = "T_STR")
public class Hello {

    @Id
    @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "t_str_seq_gen")
    @SequenceGenerator(
        name = "t_str_seq_gen",
        sequenceName = "T_STR_SEQ",
        allocationSize = 1
    )
    private Long id;


    @Column(name = "NAME", nullable = false)
    private String name;

    @Column(name = "EMAIL", nullable = false)
    private String email;
}