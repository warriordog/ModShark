FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder

COPY . /ModShark
WORKDIR /ModShark

RUN dotnet publish ModShark.sln


FROM mcr.microsoft.com/dotnet/runtime:8.0

# Install jq and use Docker caching for package downloads
ENV DEBIAN_FRONTEND=noninteractive
RUN --mount=target=/var/lib/apt/lists,type=cache,sharing=locked \
    --mount=target=/var/cache/apt,type=cache,sharing=locked \
    rm -rf /etc/apt/apt.conf.d/docker-clean && \
    apt-get update && apt-get install -y jq

# Add modshark user and copy built modshark
RUN useradd -m -s /bin/bash modshark
COPY --from=builder --chown=modshark:modshark /ModShark/ModShark/bin/Release/net8.0/publish/ /ModShark
COPY --from=builder --chown=modshark:modshark /ModShark/SharkeyDB/bin/Release/net8.0/publish/ /SharkeyDB
COPY --from=builder --chown=modshark:modshark /ModShark/entrypoint.sh /entrypoint.sh

USER modshark
WORKDIR /ModShark

# Start ModShark at container startup
ENTRYPOINT [ "/entrypoint.sh" ]
CMD [ "dotnet", "/ModShark/ModShark.dll" ]
