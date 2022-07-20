import * as ip6addr from 'ip6addr';

export class CIDRMatcher {
    /**
     * The list of IP address ranges in the classes.
     */
    private ranges: Array<ip6addr.CIDR>;

    constructor(classes: Array<string>) {
        this.ranges = [];
        classes.forEach(cidr => { this.ranges.push(ip6addr.createCIDR(cidr)); });
    }

    public contains(input: string): boolean {
        let addr: ip6addr.Addr;

        try {
            addr = ip6addr.parse(input)
        } catch {
            return false;
        }

        const elems = this.ranges.filter(cidr => cidr.contains(addr));
        return elems.length > 0;
    }
}