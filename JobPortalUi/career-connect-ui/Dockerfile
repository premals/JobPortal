FROM node:18-bookworm AS build
ARG API_URL
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
RUN npm run build

FROM node:18-bookworm-slim AS runtime
WORKDIR /app

ENV NODE_ENV=production
ENV PORT=4000
ENV SSH_PORT=2222

# Install SSH and troubleshooting tools
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
    dialog \
    openssh-server \
    ca-certificates \
    curl \
    wget \
    iputils-ping \
    dnsutils \
    net-tools \
    iproute2 \
    traceroute \
    tcpdump \
    lsof \
    procps \
    jq \
    vim-tiny \
    less \
  && rm -rf /var/lib/apt/lists/*

# Configure SSH for Azure App Service
RUN mkdir -p /var/run/sshd /run/sshd \
  && ssh-keygen -A

COPY sshd_config /etc/ssh/sshd_config

COPY package.json package-lock.json ./
RUN npm ci --omit=dev --no-audit --no-fund

COPY --from=build /app/dist ./dist

EXPOSE 4000 2222

CMD ["/bin/sh","-c",": \"${SSH_PASSWORD:=Docker!}\"; echo \"root:${SSH_PASSWORD}\" | chpasswd; /usr/sbin/sshd; exec node dist/career-connect-ui/server/server.mjs"]
