import {config} from '../config'

export const trackEvent = async (
  cid: string,
  category: string,
  action: string,
  label: string,
  value: string | number,
) => {
  const data: Record<string, string> = {
    // API Version.
    v: '1',
    // Tracking ID / Property ID.
    tid: config.ga.tid,
    // Anonymous Client Identifier. Ideally, this should be a UUID that
    // is associated with particular user, device, or browser instance.
    cid,
    // Event hit type.
    t: 'event',
    // Event category.
    ec: category,
    // Event action.
    ea: action,
    // Event label.
    el: label,
    // Event value.
    ev: value.toString(),
    // Custom value so that GA4 and above show custom events
    ua: config.ga.ua,
  }

  const url = new URL(config.ga.collectUrl)

  Object.entries(data).forEach(([key, val]) => {
    url.searchParams.append(key, val)
  })

  await fetch(url.toString(), {
    method: 'POST',
  })
}
