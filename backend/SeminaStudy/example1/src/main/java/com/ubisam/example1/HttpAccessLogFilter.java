package com.ubisam.example1;

import java.io.IOException;
import java.util.Set;
import java.util.UUID;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.MDC;
import org.springframework.core.Ordered;
import org.springframework.core.annotation.Order;
import org.springframework.stereotype.Component;
import org.springframework.util.StringUtils;
import org.springframework.web.filter.OncePerRequestFilter;

import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

@Component
@Order(Ordered.HIGHEST_PRECEDENCE)
public class HttpAccessLogFilter extends OncePerRequestFilter {

    private static final Logger log = LoggerFactory.getLogger(HttpAccessLogFilter.class);

    private static final Set<String> EXCLUDE_PREFIX = Set.of(
            "/actuator", "/swagger", "/v3/api-docs"
    );

    @Override
    protected boolean shouldNotFilter(HttpServletRequest request) {
        String uri = request.getRequestURI();
        return EXCLUDE_PREFIX.stream().anyMatch(uri::startsWith);
    }

    @Override
    protected void doFilterInternal(HttpServletRequest request,
                                    HttpServletResponse response,
                                    FilterChain filterChain)
            throws ServletException, IOException {

        long start = System.currentTimeMillis();

        String requestId = request.getHeader("X-Request-Id");
        if (!StringUtils.hasText(requestId)) {
            requestId = UUID.randomUUID().toString();
        }
        MDC.put("requestId", requestId);

        try {
            filterChain.doFilter(request, response);
        } finally {
            long timeMs = System.currentTimeMillis() - start;
            int status = response.getStatus();

            String method = request.getMethod();
            String uri = request.getRequestURI();
            String qs = request.getQueryString();

            String fullUri = (qs == null) ? uri : uri + "?" + qs;

            log.info("{} {} {} {}ms", method, fullUri, status, timeMs);

            MDC.remove("requestId");
        }
    }
}
