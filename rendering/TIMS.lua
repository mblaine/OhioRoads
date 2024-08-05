
--- Gets the z_order for a set of tags
-- @param tags OSM tags
-- @return z_order if an object with z_order, otherwise nil
function z_order(tags)
    return nil
end

--- Gets the roads table status for a set of tags
-- @param tags OSM tags
-- @return 1 if it belongs in the roads table, 0 otherwise
function roads(tags)
    return 1
end

--- Generic filtering of OSM tags
-- @param tags Raw OSM tags
-- @return Filtered OSM tags
function filter_tags_generic(tags)
    -- Short-circuit for untagged objects
    if next(tags) == nil then
        return 1, {}
    end

    return 0, tags
end

-- Filtering on nodes
function filter_tags_node (keyvalues, numberofkeys)
    return filter_tags_generic(keyvalues)
end

-- Filtering on relations
function filter_basic_tags_rel (keyvalues, numberofkeys)
    -- Filter out objects that are filtered out by filter_tags_generic
    local filter, keyvalues = filter_tags_generic(keyvalues)
    if filter == 1 then
        return 1, keyvalues
    end

    -- Filter out all relations except route, multipolygon and boundary relations
    if ((keyvalues["type"] ~= "route") and (keyvalues["type"] ~= "multipolygon") and (keyvalues["type"] ~= "boundary")) then
        return 1, keyvalues
    end

    return 0, keyvalues
end

-- Filtering on ways
function filter_tags_way (keyvalues, numberofkeys)
    local filter = 0  -- Will object be filtered out?
    local polygon = 0 -- Will object be treated as polygon?

    -- Filter out objects that are filtered out by filter_tags_generic
    filter, keyvalues = filter_tags_generic(keyvalues)
    if filter == 1 then
        return filter, keyvalues, polygon, roads
    end

    polygon = isarea(keyvalues)

    -- Add z_order column
    keyvalues["z_order"] = z_order(keyvalues)

    return filter, keyvalues, polygon, roads(keyvalues)
end


--- Handling for relation members and multipolygon generation
-- @param keyvalues OSM tags, after processing by relation transform
-- @param keyvaluemembers OSM tags of relation members, after processing by way transform
-- @param roles OSM roles of relation members
-- @param membercount number of members
-- @return filter, cols, member_superseded, boundary, polygon, roads
function filter_tags_relation_member (keyvalues, keyvaluemembers, roles, membercount)
    local members_superseded = {}

    -- Start by assuming that this not an old-style MP
    for i = 1, membercount do
        members_superseded[i] = 0
    end

    local type = keyvalues["type"]

    -- Remove type key
    keyvalues["type"] = nil

    -- Filter out relations with just a type tag or no tags
    if next(keyvalues) == nil then
        return 1, keyvalues, members_superseded, 0, 0, 0
    end

    if type == "boundary" or (type == "multipolygon" and keyvalues["boundary"]) then
        keyvalues.z_order = z_order(keyvalues)
        return 0, keyvalues, members_superseded, 1, 0, roads(keyvalues)
    -- For multipolygons...
    elseif (type == "multipolygon") then
        -- Multipolygons by definition are polygons, so we know roads = linestring = 0, polygon = 1
        keyvalues.z_order = z_order(keyvalues)
        return 0, keyvalues, members_superseded, 0, 1, 0
    elseif type == "route" then
        keyvalues.z_order = z_order(keyvalues)
        return 0, keyvalues, members_superseded, 1, 0, roads(keyvalues)
    end

    -- Unknown type of relation or no type tag
    return 1, keyvalues, members_superseded, 0, 0, 0
end

--- Check if an object with given tags should be treated as polygon
-- @param tags OSM tags
-- @return 1 if area, 0 if linear
function isarea (tags)
    return 0
end

function is_in (needle, haystack)
    for index, value in ipairs (haystack) do
        if value == needle then
            return true
        end
    end
    return false
end

--- Normalizes layer tags
-- @param v The layer tag value
-- @return An integer for the layer tag
function layer (v)
    return v and string.find(v, "^-?%d+$") and tonumber(v) < 100 and tonumber(v) > -100 and v or nil
end