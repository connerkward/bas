"""
NDI Discovery + UUID Parsing for TouchDesigner

Usage in TD:
- Import in Script DAT or Execute DAT
- Call discover_participants() to get active streams + UUIDs

Requires: ndi-python (pip install ndi-python)
"""

import re
from typing import Dict, List, Optional

# Pattern for BAS participant streams
NDI_STREAM_PATTERN = re.compile(r'^BAS_Participant_([a-f0-9]{8})$')


def get_ndi_sources() -> List[str]:
    """
    Get all available NDI sources.
    Returns list of source names.
    """
    try:
        import NDIlib as ndi
        
        if not ndi.initialize():
            print("NDI: Failed to initialize")
            return []
        
        finder = ndi.find_create_v2()
        if finder is None:
            print("NDI: Failed to create finder")
            return []
        
        # Wait briefly for sources to be discovered
        ndi.find_wait_for_sources(finder, 1000)  # 1 second timeout
        sources = ndi.find_get_current_sources(finder)
        
        source_names = [s.ndi_name for s in sources] if sources else []
        
        ndi.find_destroy(finder)
        return source_names
        
    except ImportError:
        print("NDI: ndi-python not installed")
        return []
    except Exception as e:
        print(f"NDI: Error discovering sources: {e}")
        return []


def parse_uuid_from_stream(stream_name: str) -> Optional[str]:
    """
    Extract UUID from BAS_Participant_<UUID> stream name.
    Returns None if not a BAS participant stream.
    """
    # Stream names may include host: "HOSTNAME (BAS_Participant_abc12345)"
    # Extract just the stream portion
    if '(' in stream_name:
        stream_name = stream_name.split('(')[-1].rstrip(')')
    
    match = NDI_STREAM_PATTERN.match(stream_name)
    return match.group(1) if match else None


def discover_participants() -> Dict[str, str]:
    """
    Discover all BAS participant NDI streams.
    
    Returns dict: {uuid: full_stream_name}
    """
    sources = get_ndi_sources()
    participants = {}
    
    for source in sources:
        uuid = parse_uuid_from_stream(source)
        if uuid:
            participants[uuid] = source
    
    return participants


def get_participant_uuids() -> List[str]:
    """
    Get list of active participant UUIDs.
    """
    return list(discover_participants().keys())


# For TD Execute DAT: call on frame
def onFrameStart(frame):
    """
    Example TD callback - update participant list each frame.
    """
    participants = discover_participants()
    # Store in TD global storage or update operators
    return participants


if __name__ == "__main__":
    # Test discovery
    print("Discovering NDI sources...")
    sources = get_ndi_sources()
    print(f"Found {len(sources)} sources:")
    for s in sources:
        print(f"  - {s}")
    
    print("\nBAS Participants:")
    participants = discover_participants()
    for uuid, stream in participants.items():
        print(f"  - UUID: {uuid} -> {stream}")
