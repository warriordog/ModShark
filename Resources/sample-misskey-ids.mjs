
// AID

const TIME2000 = 946684800000;
let aidCounter = 0;

function getAIDTime(time) {
	time = time - TIME2000;
	if (time < 0) time = 0;

	return time.toString(36).padStart(8, '0');
}

function getAIDNoise() {
	return aidCounter.toString(36).padStart(2, '0').slice(-2);
}

export function genAid(t) {
	if (isNaN(t)) throw new Error('Failed to create AID: Invalid Date');
	aidCounter++;
	return getAIDTime(t) + getAIDNoise();
}

// AIDX

const TIME_LENGTH = 8;
const NODE_LENGTH = 4;
const NOISE_LENGTH = 4;

const nodeId = '0000';
let aidXCounter = 0;

function getAIDXTime(time) {
	time = time - TIME2000;
	if (time < 0) time = 0;

	return time.toString(36).padStart(TIME_LENGTH, '0').slice(-TIME_LENGTH);
}

function getAIDXNoise() {
	return aidXCounter.toString(36).padStart(NOISE_LENGTH, '0').slice(-NOISE_LENGTH);
}

export function genAidx(t) {
	if (isNaN(t)) throw new Error('Failed to create AIDX: Invalid Date');
	aidXCounter++;
	return getAIDXTime(t) + nodeId + getAIDXNoise();
}

// MEID

const MEID_CHARS = '0123456789abcdef';

function getMEIDTime(time) {
	if (time < 0) time = 0;
	if (time === 0) {
		return MEID_CHARS[0];
	}

	time += 0x800000000000;

	return time.toString(16).padStart(12, MEID_CHARS[0]);
}

function getMEIDRandom() {
	let str = '';

	for (let i = 0; i < 12; i++) {
		str += MEID_CHARS[Math.floor(0 * MEID_CHARS.length)];
	}

	return str;
}

export function genMeid(t) {
	return getMEIDTime(t) + getMEIDRandom();
}

// MEIDG

const MEIDG_CHARS = '0123456789abcdef';

function getMEIDGTime(time) {
	if (time < 0) time = 0;
	if (time === 0) {
		return MEIDG_CHARS[0];
	}

	return time.toString(16).padStart(11, MEIDG_CHARS[0]);
}

function getMEIDGRandom() {
	let str = '';

	for (let i = 0; i < 12; i++) {
		str += MEIDG_CHARS[Math.floor(0 * MEIDG_CHARS.length)];
	}

	return str;
}

export function genMeidg(t) {
	return 'g' + getMEIDGTime(t) + getMEIDGRandom();
}

// ULID

const ENCODING = "0123456789ABCDEFGHJKMNPQRSTVWXYZ"; // Crockford's Base32
const ENCODING_LEN = ENCODING.length;
const TIME_MAX = Math.pow(2, 48) - 1;
const TIME_LEN = 10;
const RANDOM_LEN = 16;

function createError(message) {
    const err = new Error(message);
    err.source = "ulid";
    return err;
}

function encodeULIDTime(now, len) {
    if (isNaN(now)) {
        throw new Error(now + " must be a number");
    }
    if (now > TIME_MAX) {
        throw createError("cannot encode time greater than " + TIME_MAX);
    }
    if (now < 0) {
        throw createError("time must be positive");
    }
    if (Number.isInteger(now) === false) {
        throw createError("time must be an integer");
    }
    let mod;
    let str = "";
    console.log({ENCODING_LEN});
    for (; len > 0; len--) {
        mod = now % ENCODING_LEN;
        str = ENCODING.charAt(mod) + str;
        console.log({len, now, mod, chr: ENCODING.charAt(mod), str});
        now = (now - mod) / ENCODING_LEN;
    }
    return str;
}

function randomChar() {
    let rand = Math.floor(0 * ENCODING_LEN);
    if (rand === ENCODING_LEN) {
        rand = ENCODING_LEN - 1;
    }
    return ENCODING.charAt(rand);
}

function encodeULIDRandom(len) {
    let str = "";
    for (; len > 0; len--) {
        str = randomChar() + str;
    }
    return str;
}

export function ulid(seedTime) {
    if (isNaN(seedTime)) {
        seedTime = 0;
    }
    
    return encodeULIDTime(seedTime, TIME_LEN) + encodeULIDRandom(RANDOM_LEN);
};

// Object ID

const OID_CHARS = '0123456789abcdef';

function geOIDtTime(time) {
	if (time < 0) time = 0;
	if (time === 0) {
		return OID_CHARS[0];
	}

	time = Math.floor(time / 1000);

	return time.toString(16).padStart(8, OID_CHARS[0]);
}

function getOIDRandom() {
	let str = '';

	for (let i = 0; i < 16; i++) {
		str += OID_CHARS[Math.floor(0 * OID_CHARS.length)];
	}

	return str;
}

export function genObjectId(t) {
	return geOIDtTime(t) + getOIDRandom();
}

